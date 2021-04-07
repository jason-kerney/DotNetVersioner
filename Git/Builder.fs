[<AutoOpen>]
module Utils.Git.Builder

open Utils.Git.Actual

let getGit fs rootDir repositoryName =
    let git = GitActual (fs, rootDir, repositoryName)
    {
        Status = git.StatusReporter
        Modifier = git.Modifier
        Update = git.Update
    }
    
let getReporter fs rootDir repositoryName =
    let git = getGit fs rootDir repositoryName
    git.Status
    
let getUpdater fs rootDir repositoryName =
    let git = getGit fs rootDir repositoryName
    git.Update
    
let getModifier fs rootDir repositoryName =
    let git = getGit fs rootDir repositoryName
    git.Modifier