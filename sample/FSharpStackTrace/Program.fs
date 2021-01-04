// Learn more about F# at http://fsharp.org

open System
open System.Diagnostics
open FSharp.Control.Tasks

let call i = async {
    do! Async.Sleep 1
    if i = 10 then
        failwith "BOOM!"
    return i
}

let run count = async {
    let calls = Array.init count call
    for call in calls do
        let! _ = call
        ()
    return 0
}

let makeTheCall () = task {
    let! x = run 20
    return x
}

[<EntryPoint>]
let main argv =
    try
        let results = makeTheCall().GetAwaiter().GetResult()
        printfn "%A" results
    with e ->
        printfn "%s" <| string (e.Demystify())
    0 // return an integer exit code
