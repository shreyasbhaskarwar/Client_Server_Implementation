open System
open System.IO
open System.Net
open System.Net.Sockets
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Control

let serverPort = 12345
let ipAddress = "127.0.0.1"
let mutable terminate = false
let VALID_FUNCTIONS = [ "add"; "subtract"; "multiply" ]
// Minimum words in the input - 1 command + 2 inputs = 3
let MIN_WORDS = 3
// Maximum words in the input - 1 command + 4 inputs = 5
let MAX_WORDS = 5

let validateInput (input :string) :int =
    let words :string[] = input.Split ' '
    if VALID_FUNCTIONS |> List.contains words[0] |> not
    then
        -1
    elif words.Length < MIN_WORDS
    then
        -2
    elif words.Length > MAX_WORDS
    then
        -3
    elif Array.exists (fun (x :string) -> not (x |> Int32.TryParse |> fst)) words[1..]
    then
        -4
    else
        0

let calculateResult (command :string) :int =
    let words :string[] = command.Split ' '
    match words[0] with
    | "add" -> Array.sumBy int words[1..]
    | "subtract" -> words[2..] |> Array.fold (fun acc x -> acc - int(x)) (int(words[1]))
    | "multiply" -> Array.fold (fun acc x -> acc * int(x)) 1 words[1..]
    
let handleClient (clientSocket: TcpClient, clientId: int) =
    async {
        let stream = clientSocket.GetStream()
        let reader = new StreamReader(stream)
        let writer = new StreamWriter(stream)
        
        // Send the hello response to the client
        do! writer.WriteLineAsync("Hello!") |> Async.AwaitTask
        do! writer.FlushAsync() |> Async.AwaitTask
        
        let mutable response = Int32.MinValue
        
        while response <> -5 do
            // Terminate condition to exit all child threads
            if terminate
            then
                response <- -5
            
            if response <> -5
            then
            // Read the command send from client
                let! command = reader.ReadLineAsync() |> Async.AwaitTask
                Console.WriteLine($"Received: {command}")

                // Process the command here and generate a result
                if command = "terminate"
                then
                    response <- -5
                    terminate <- true
                elif command = "bye"
                then
                    response <- -5
                else
                    let validationResult = validateInput command
                    if validationResult <> 0
                    then
                        response <- validationResult
                    else
                        let result = calculateResult command
                        response <- result
                        
                // Send the result back to the client
                Console.WriteLine($"Responding to client{clientId} with result: {response}")
                do! writer.WriteLineAsync(string(response)) |> Async.AwaitTask
                do! writer.FlushAsync() |> Async.AwaitTask

        clientSocket.GetStream().Close()
        clientSocket.Close()
        clientSocket.Dispose()
    }

let startServer () =
    async {
        let ipAddress = IPAddress.Parse(ipAddress)
        let listener = TcpListener(ipAddress, serverPort)
        let mutable clientId = 1
        
        listener.Start()
        Console.WriteLine($"Server is running and listening on port {serverPort}.")

        while not terminate do
            if (listener.Pending())
            then
                let! client = listener.AcceptTcpClientAsync() |> Async.AwaitTask
                let! clientTask = handleClient (client, clientId) |> Async.StartChild
                clientTask |> ignore
                clientId <- clientId + 1
            
        listener.Stop()
    }

[<EntryPoint>]
let main args =
    startServer () |> Async.RunSynchronously
    0