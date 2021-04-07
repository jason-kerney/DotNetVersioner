module internal Utils.Git.Process
open System.Diagnostics
open System.IO
open Utils.Maybe
open Utils.Strings

let start (workingDir: string) command args =
    let getOutPut (curProcess: #StreamReader) acc =
        let rec getOutPut (curProcess: #StreamReader) (acc: string list) =
                if curProcess.EndOfStream then
                    acc |> List.rev
                else
                    (curProcess.ReadLine ()) :: acc
                    |> getOutPut curProcess
            
        getOutPut curProcess acc
    
    let rec getStandardOutput (curProcess: Process) acc =
        getOutPut curProcess.StandardOutput acc
            
    let rec getStandardError (curProcess: Process) acc =
        getOutPut curProcess.StandardError acc
            
    let getDisplay proc =
            []
            |> getStandardOutput proc 
            |> getStandardError proc
     
    async {
        use curProcess = new Process ()
        let processInfo = ProcessStartInfo (command, args)
        processInfo.UseShellExecute <- false
        processInfo.RedirectStandardOutput <- true
        processInfo.RedirectStandardError <- true
        processInfo.CreateNoWindow <- true
        processInfo.WorkingDirectory <- workingDir
        curProcess.StartInfo <- processInfo
        return
             maybe {
                let id = sprintf "%s %s" command args
                curProcess.Start () |> ignore
                
                return
                    id,
                    getDisplay curProcess
                    |> join
            }
    }
