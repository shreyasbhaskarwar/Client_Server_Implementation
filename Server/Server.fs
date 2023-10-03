open System
open System.IO
open System.Net
open System.Net.Sockets
open Microsoft.FSharp.Control

let serverPort = 12345
let ipAddress = "127.0.0.1"

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
            // Read the command send from client
            let! request = reader.ReadLineAsync() |> Async.AwaitTask
            Console.WriteLine($"Received: {request}")

            // Process the command here and generate a result
            if request = "bye"
            then
                response <- -5
                
            // Send the response back to the client
            Console.WriteLine($"Responding to client{clientId} with result: {response}")
            do! writer.WriteLineAsync(string(response)) |> Async.AwaitTask
            do! writer.FlushAsync() |> Async.AwaitTask

        clientSocket.Close()
    }

let startServer () =
    async {
        let ipAddress = IPAddress.Parse(ipAddress)
        let listener = new TcpListener(ipAddress, serverPort)
        let mutable clientId = 1
        
        listener.Start()
        Console.WriteLine($"Server is running and listening on port {serverPort}.")

        while true do
            let! client = listener.AcceptTcpClientAsync() |> Async.AwaitTask
            let! clientTask = handleClient (client, clientId) |> Async.StartChild
            clientTask |> ignore
            clientId <- clientId + 1
    }

[<EntryPoint>]
let main args =
    startServer () |> Async.RunSynchronously
    0