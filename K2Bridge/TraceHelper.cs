﻿namespace K2Bridge
{
    using System;
    using System.IO;

    internal class TraceHelper
    {
        private readonly Serilog.ILogger logger;

        private readonly string tracePath;

        internal TraceHelper(Serilog.ILogger logger, string tracePath)
        {
            this.logger = logger;
            this.tracePath = tracePath;

            // Setup tracing directory
            if (!Directory.Exists(tracePath))
            {
                Directory.CreateDirectory(tracePath);
            }
        }

        internal void WriteFile(string filename, string content)
        {
            try
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(this.tracePath, filename)))
                {
                    outputFile.Write(content);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Failed to write trace files.");
                this.logger.Warning($"Create folder {this.tracePath} to dump the content of translated queries");
            }
        }

        internal void WriteFile(string filename, Stream content)
        {
            try
            {
                using (FileStream outputFile = new FileStream(Path.Combine(this.tracePath, filename), FileMode.CreateNew))
                {
                    StreamUtils.CopyStream(content, outputFile);
                    outputFile.Flush();
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Failed to write trace files.");
                this.logger.Warning($"Create folder {this.tracePath} to dump the content of translated queries");
            }
        }

    }
}
