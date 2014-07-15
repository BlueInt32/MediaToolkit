﻿using System;
using System.Text;
using MediaToolkit.Model;
using MediaToolkit.Options;
using MediaToolkit.Util;

namespace MediaToolkit
{
    internal class CommandBuilder
    {
        internal static string Serialize(Engine.EngineParameters engineParameters)
        {
            switch (engineParameters.Task)
            {
                case Engine.FFmpegTask.Convert:
                    return Convert(engineParameters.InputFile, engineParameters.OutputFile, engineParameters.ConversionOptions);

                case Engine.FFmpegTask.GetMetaData:
                    return GetMetadata(engineParameters.InputFile);

                case Engine.FFmpegTask.GetThumbnail:
                    return GetThumbnail(engineParameters.InputFile, engineParameters.OutputFile, engineParameters.ConversionOptions);
            }
            return null;
        }

        private static string GetMetadata(MediaFile inputFile)
        {
            return string.Format("-i \"{0}\" ", inputFile.Filename);
        }

        private static string GetThumbnail(MediaFile inputFile, MediaFile outputFile, ConversionOptions conversionOptions)
        {
            var commandBuilder = new StringBuilder();
            
            commandBuilder.AppendFormat(" -ss {0} ",
                conversionOptions.Seek.GetValueOrDefault(TimeSpan.FromSeconds(1)).TotalSeconds);
            
            commandBuilder.AppendFormat(" -i \"{0}\" ", inputFile.Filename);
            commandBuilder.AppendFormat(" -t {0} ", 1);
            commandBuilder.AppendFormat(" -vframes {0} ", 1);

			// Video size / resolution
			if (conversionOptions.VideoSize != VideoSize.Default)
			{
				string size = conversionOptions.VideoSize.ToLower();
				if (size.StartsWith("_")) size = size.Replace("_", "");
				if (size.Contains("_")) size = size.Replace("_", "-");
				if (size.Contains("par")) size = size.Replace("par", "*");

				commandBuilder.AppendFormat(" -s {0} ", size);
			}

            return commandBuilder.AppendFormat(" \"{0}\" ", outputFile.Filename).ToString();
        }

        private static string Convert(MediaFile inputFile, MediaFile outputFile, ConversionOptions conversionOptions)
        {
            var commandBuilder = new StringBuilder();

            // Default conversion
            if (conversionOptions == null)
                return commandBuilder.AppendFormat(" -i \"{0}\"  \"{1}\" ",inputFile.Filename, outputFile.Filename).ToString();

            // Media seek position
            if (conversionOptions.Seek != null)
                commandBuilder.AppendFormat(" -ss {0} ", conversionOptions.Seek.Value.TotalSeconds);

            commandBuilder.AppendFormat(" -i \"{0}\" ", inputFile.Filename);

            // Physical media conversion (DVD etc)
            if (conversionOptions.Target != Target.Default)
            {
                commandBuilder.Append(" -target ");
                if (conversionOptions.TargetStandard != TargetStandard.Default)
                {
                    commandBuilder.AppendFormat(" {0}-{1} \"{2}\" ", conversionOptions.TargetStandard.ToLower(),
                        conversionOptions.Target.ToLower(), outputFile.Filename);

                    return commandBuilder.ToString();
                }
                commandBuilder.AppendFormat("{0} \"{1}\" ", conversionOptions.Target.ToLower(), outputFile.Filename);

                return commandBuilder.ToString();
            }

            // Audio sample rate
            if (conversionOptions.AudioSampleRate != AudioSampleRate.Default)
                commandBuilder.AppendFormat(" -ar {0} ", conversionOptions.AudioSampleRate.Remove("Hz"));

            // Maximum video duration
            if (conversionOptions.MaxVideoDuration != null)
                commandBuilder.AppendFormat(" -t {0} ", conversionOptions.MaxVideoDuration);

            // Video bit rate
            if (conversionOptions.VideoBitRate != null)
                commandBuilder.AppendFormat(" -b {0}k ", conversionOptions.VideoBitRate);

            // Video size / resolution
            if (conversionOptions.VideoSize != VideoSize.Default)
            {
                string size = conversionOptions.VideoSize.ToLower();
                if (size.StartsWith("_")) size = size.Replace("_", "");
                if (size.Contains("_")) size = size.Replace("_", "-");
				if (size.Contains("par")) size = size.Replace("par", "*");

                commandBuilder.AppendFormat(" -s {0} ", size);
            }

            // Video aspect ratio
            if (conversionOptions.VideoAspectRatio != VideoAspectRatio.Default)
            {
                string ratio = conversionOptions.VideoAspectRatio.ToString();
                ratio = ratio.Substring(1);
                ratio = ratio.Replace("_", ":");

                commandBuilder.AppendFormat(" -aspect {0} ", ratio);
            }

            return commandBuilder.AppendFormat(" \"{0}\" ", outputFile.Filename).ToString();
        }
    }
}