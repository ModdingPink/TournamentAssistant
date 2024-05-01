﻿// Define the path to the existing executable
using System.Diagnostics;

if (args.Length <= 0)
{
    Console.WriteLine("You shouldn't be seeing this, but anyway, you must provide a parameter to this program which is the path of the calling executable");
    await Task.Delay(10000);
    return;
}

var index = 0;
foreach (var arg in args)
{
    Console.WriteLine($"{index++}: {arg}");
}

Console.WriteLine("Updating TA...");

if (args[0] == "-taui") // TAUpdater.exe -taui [path to taui.exe]
{
    var existingPath = args[1];

    // Define the URL to download the new executable
    var url = "http://tournamentassistant.net/downloads/taui.exe";

    try
    {
        // Attempt to delete the existing file, waiting up to 5 seconds if it is still running
        var fileDeleted = false;
        var retryCount = 0;
        const int maxRetryCount = 5;

        while (!fileDeleted && retryCount < maxRetryCount)
        {
            try
            {
                if (File.Exists(existingPath))
                {
                    File.Delete(existingPath);
                    fileDeleted = true;
                    Console.WriteLine("Existing file deleted");
                }
                else
                {
                    fileDeleted = true; // File does not exist, no need to delete
                    Console.WriteLine("No existing file found to delete");
                }
            }
            catch (IOException)
            {
                // Wait for 1 second before trying again
                await Task.Delay(1000);
                retryCount++;
            }
        }

        if (!fileDeleted)
        {
            Console.WriteLine("Failed to delete the existing file after 5 seconds");
            return; // Exit if the file cannot be deleted
        }

        // Download the update using HttpClient
        using (var client = new HttpClient())
        using (var fileStream = new FileStream(existingPath, FileMode.CreateNew))
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var contentStream = await response.Content.ReadAsStreamAsync();
            await contentStream.CopyToAsync(fileStream);
            Console.WriteLine("Update downloaded");
        }

        // Execute the downloaded file
        Process.Start(existingPath);
        Console.WriteLine("Launched new TAUI");
    }
    catch (Exception ex)
    {
        // Handle any errors that might have occurred
        Console.WriteLine("An error occurred: " + ex.Message);
    }
}
else if (args[0] == "-plugin") // TAUpdater.exe -plugin [path to Beat Saber installation] [beat saber command line args, for relaunch]
{
    var beatSaberDirectory = args[1];
    beatSaberDirectory = Path.GetFullPath(beatSaberDirectory);

    var destinationFileName = "TournamentAssistant.dll";
    var destinationDirectory = Path.GetFullPath($"{beatSaberDirectory}/IPA/Pending/Plugins/");
    var destinationPath = Path.Combine(destinationDirectory, destinationFileName);
    var beatSaberExecutable = Path.Combine(beatSaberDirectory, "Beat Saber.exe");

    // Create IPA/Pending/Plugins if it doesn't yet exist
    Directory.CreateDirectory(destinationDirectory);

    // Define the URL to download the new executable
    var url = "http://tournamentassistant.net/downloads/TournamentAssistant.dll";

    try
    {
        // Download the update using HttpClient
        using (var client = new HttpClient())
        using (var fileStream = new FileStream(destinationPath, FileMode.Create))
        {
            var response = await client.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            using var contentStream = await response.Content.ReadAsStreamAsync();
            await contentStream.CopyToAsync(fileStream);
            Console.WriteLine("Update downloaded");
        }

        // Relaunch beat saber
        var argsAsString = string.Join(" ", args);
        var beatSaberCommand = argsAsString.Substring(argsAsString.IndexOf("-commandLine") + "-commandLine ".Length);

        // Splitting this out because we can't trust beatSaberCommand to have the executable path escaped
        var beatSaberParameters = beatSaberCommand.Substring(beatSaberCommand.IndexOf("Beat Saber.exe ") + "Beat Saber.exe ".Length);

        var startInfo = new ProcessStartInfo(beatSaberExecutable)
        {
            Arguments = beatSaberParameters,
            UseShellExecute = true,
            CreateNoWindow = false, // This should be redundant with UseShellExecute as true
            WindowStyle = ProcessWindowStyle.Normal,
            WorkingDirectory = beatSaberDirectory
        };

        Process.Start(startInfo);
        Console.WriteLine($"Relaunched Beat Saber as: {beatSaberCommand}");
    }
    catch (Exception ex)
    {
        // Handle any errors that might have occurred
        Console.WriteLine("An error occurred: " + ex.Message);
    }
}

await Task.Delay(10000);