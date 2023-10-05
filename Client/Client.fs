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
        let mutable result = ""
        let mutable terminate = false
        
        let! welcomeMessage = reader.ReadLineAsync() |> Async.AwaitTask
        Console.WriteLine($"{welcomeMessage}")
        
        while result <> "exit" do
            let command = Console.ReadLine()
            Console.WriteLine($"Sending command: {command}")
            do! writer.WriteLineAsync(command) |> Async.AwaitTask
            do! writer.FlushAsync() |> Async.AwaitTask
            
            if command = "terminate"
            then
                terminate <- true
            
            let! response = reader.ReadLineAsync() |> Async.AwaitTask
            match response with
            | "-1" -> result <- "incorrect operation command" 
            | "-2" -> result <- "number of inputs is less than two" 
            | "-3" -> result <- "number of inputs is more than four" 
            | "-4" -> result <- "one or more of the inputs contain(s) non-number(s)"
            | "-5" -> result <- "exit" 
            | _ -> result <- response
            
            if not terminate
            then
                if result <> "exit"
                then
                    Console.WriteLine($"Server response: {result}")
                else
                    Console.WriteLine($"result")
            
        client.GetStream().Close()
        client.Close()
        client.Dispose()
    }

[<EntryPoint>]
let main args =
    startClient () |> Async.RunSynchronously
    0
