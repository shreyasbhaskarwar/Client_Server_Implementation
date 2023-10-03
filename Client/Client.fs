open System
open System.IO
open System.Net.Sockets

let serverAddress = "127.0.0.1"
let serverPort = 12345

let startClient () =
    async {
        let client = new TcpClient(serverAddress, serverPort)
        let stream = client.GetStream()
        let reader = new StreamReader(stream)
        let writer = new StreamWriter(stream)
        let mutable result = -Int32.MinValue
        
        let! welcomeMessage = reader.ReadLineAsync() |> Async.AwaitTask
        Console.WriteLine($"{welcomeMessage}")
        
        while result <> -5 do
            let command = Console.ReadLine()
            Console.WriteLine($"Sending command: {command}")
            do! writer.WriteLineAsync(command) |> Async.AwaitTask
            do! writer.FlushAsync() |> Async.AwaitTask
            
            let! response = reader.ReadLineAsync() |> Async.AwaitTask
            result <- int(response)
            Console.WriteLine($"Server response: {result}")
            
        client.Close() 
    }

[<EntryPoint>]
let main args =
    startClient () |> Async.RunSynchronously
    0
