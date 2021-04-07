module Version.DotNet.Finders.BuildProps

open System
open System.Collections.Generic
open System.IO
open System.Text
open System.Xml

open Utils.FileSystem
open Utils.FileSystem.Helpers
open Version.DotNet.Parser
open Version.DotNet.Types
open Utils.Maybe

let private parentName = "PropertyGroup"
let private versionKey = "Version"
let private prefixKey = "VersionPrefix"
let private suffixKey = "VersionSuffix"
let private fileVersionKey = "FileVersion"
let private infoVersionKey = "InformationalVersion"

let private getXml raw =
    let doc = XmlDocument () in
        doc.LoadXml raw
        
    let propGroups =
        seq {
            for propGroup in doc.DocumentElement.ChildNodes do
                if propGroup.Name = parentName then
                    yield propGroup
        }
                    
    let primaryElements =
        let keys =
            [
                versionKey
                prefixKey
                suffixKey
                fileVersionKey
                infoVersionKey
            ]
            
        seq {
            for propGroup in propGroups do
                for child in propGroup.ChildNodes do
                    if keys |> List.contains child.Name then
                        yield (child.Name, (propGroup, child))
        }
        |> readOnlyDict
        
    doc, propGroups, primaryElements
    
let private getInnerText (xml: XmlNode) = xml.InnerText
    
let private tryRead (elements: IReadOnlyDictionary<string, (XmlNode * XmlNode)>) key =
    if elements.ContainsKey key then elements.[key] |> snd |> getInnerText |> Some
    else None

let private setValue (document: XmlDocument) (elements: IReadOnlyDictionary<string, (XmlNode * XmlNode)>) (parents: XmlNode seq) key text =
    let target =
        if elements.ContainsKey key then
            elements.[key] |> snd
            
        else
            let parent =
                parents
                |> Seq.head
                
            let element = document.CreateElement key
            parent.InsertAfter (element, parent.LastChild)
          
    target.InnerText <- text
    
let private getFormattedXml (document: XmlDocument) =
    use memory = new MemoryStream ()
    use writer = new XmlTextWriter(memory, Encoding.Unicode) in
                 writer.Formatting <- Formatting.Indented
                 
    document.WriteContentTo(writer)
    writer.Flush ()
    memory.Flush ()
    
    memory.Position <- 0L
    
    use reader = new StreamReader (memory)
    
    reader.ReadToEnd ()
    
let private updateExistingFile (file: IFileWrapper) (version: Version) =
                maybe {
                let! text =
                    file
                    |> readAllText
                    
                let doc, propGroups, primaryElements = getXml text
                let setVersionPart = setValue doc primaryElements propGroups
                    
                if primaryElements.ContainsKey versionKey then
                    let (parent, child) = primaryElements.[versionKey]
                    parent.RemoveChild child |> ignore
                    
                    
                () |> version.ToMainVersionString |> setVersionPart prefixKey
                
                if 0 < version.PrereleaseId.Length then
                    version.PrereleaseId |> setVersionPart suffixKey
                    
                elif primaryElements.ContainsKey suffixKey then
                    let (parent, child) = primaryElements.[suffixKey]
                    parent.RemoveChild child |> ignore
                    
                () |> version.ToFullVersionString |> setVersionPart fileVersionKey
                () |> version.ToFullVersionString |> setVersionPart infoVersionKey
                
                return! doc |> getFormattedXml |> Ok |> writeAllText file
            }

type BuildPropsFinder (path: string, fs: IFileSystemAccessor) =
    let getValueOrEmpty = getValueOrDefault String.Empty
        
    let fileName = "Directory.Build.props"
                
    interface IUpdatable with
            
        member this.Name with get () =
            match this.FullPath with
            | Some path -> path
            | _ -> fileName
            
        member this.Directory with get () = path |> fs.Directory
                
        member this.GetVersion () =
            let path = 
                match this.FullPath with
                | None ->
                    $"{path}{System.IO.Path.DirectorySeparatorChar}{fileName} does not exist"
                    |> asGeneralFailure
                | Some path -> Ok path
              
            let resultString = 
                path
                |> fs.MFile |> maybeReadAllText
                
            maybe {
                let! rawXml = resultString
                let _, _, elements = getXml rawXml
                let tryGetValue = tryRead elements
                
                let version = versionKey |> tryGetValue  |> getValueOrEmpty
                let prefix = prefixKey |> tryGetValue |> getValueOrEmpty
                let suffix = suffixKey |> tryGetValue |> getValueOrEmpty
                
                let versionString =
                    let baseVersion =
                        if 0 < prefix.Length then
                            prefix
                        else
                            version
                            
                    let suffix =
                        if 0 < suffix.Length then $".{suffix}"
                        else String.Empty    
                        
                            
                    $"{baseVersion}{suffix}"
                    
                return versionString |> parseVersion suffix
            }
            
        member this.UpdateVersion create version =
            maybe {
                
                if this.FullPath |> hasValue then
                    let! file = 
                        this.FullPath
                        |> Maybe.fromOption $"No {fileName} found"
                        |> fs.MFile
                        
                    let! _success = version |> updateExistingFile file
                    ()
                elif create then
                    let file = fileName |> fs.JoinF path 
                    let! _success = "<Project><PropertyGroup></PropertyGroup></Project>" |> Ok |> file.WriteAllText
                    let! _success = version |> updateExistingFile file
                    ()
                    
                return version
            }
                
    member this.FullPath with get () =
        let file =
            fileName
            |> fs.JoinF path
            
        if file.Exists then
            file.FullName |> Some
        else None
        
let getBuildPropsVersionFinder fs path =
    BuildPropsFinder (path, fs) :> IUpdatable