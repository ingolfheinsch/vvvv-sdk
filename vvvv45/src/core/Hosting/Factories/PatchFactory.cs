﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Factories
{
    /// <summary>
    /// Effects factory, parses and watches the effect directory
    /// </summary>
    [Export(typeof(IAddonFactory))]
    [ComVisible(false)]
    public class PatchFactory : AbstractFileFactory<INode>
    {
        private string FDTD = "";
        
        [Import]
        protected ILogger Logger { get; set; }
        
        public PatchFactory()
            : base(".v4p;.v4x")
        {
        }
        
        public override string JobStdSubPath {
            get {
                return "modules";
            }
        }

        //create a node info from a filename
        protected override IEnumerable<INodeInfo> LoadNodeInfos(string filename)
        {
            if (FDTD == "") LoadDTD();

            //check filename structure
            string fn = Path.GetFileNameWithoutExtension(filename);
            
            INodeInfo nodeInfo;
            //check filename structure to see if this is a module or an ordinary patch
            if (Regex.IsMatch(fn, @"^.+\s\(.+\)$"))
            {
                //match the filename
                var match = Regex.Match(fn, @"(\S+) \((\S+)(?: ([^)]*))?\)");
                
                //create node info and read matches
                nodeInfo = FNodeInfoFactory.CreateNodeInfo(
                    match.Groups[1].Value,
                    match.Groups[2].Value,
                    match.Groups[3].Value,
                    filename,
                    true);
                
                nodeInfo.Type = NodeType.Module;
                nodeInfo.InitialComponentMode = TComponentMode.InAWindow; // Hidden;
            }
            else //patch
            {
                //create node info and read matches
                nodeInfo = FNodeInfoFactory.CreateNodeInfo(fn, "", "", filename, true);
                
                nodeInfo.Type = NodeType.Patch;
                nodeInfo.InitialComponentMode = TComponentMode.InAWindow;
            }
            
            nodeInfo.Factory = this;
            
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(filename))
                {
                    //skip first line
                    var s = sr.ReadLine();
                    
                    var settings = new XmlReaderSettings();
                    settings.ProhibitDtd = false;
                    
                    using (StringReader stringReader = new StringReader(FDTD + sr.ReadToEnd()))
                    {
                        var xmlReader = XmlReader.Create(stringReader, settings);
                        //xmlReader.Settings
                        if(xmlReader.ReadToFollowing("INFO"))
                        {
                            nodeInfo.Author = xmlReader.GetAttribute("author");
                            nodeInfo.Help = xmlReader.GetAttribute("description");
                            nodeInfo.Tags = xmlReader.GetAttribute("tags");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Could not extract detailed module info from XML of " + nodeInfo.Systemname);
                Logger.Log(e);
            }
            finally
            {
                nodeInfo.CommitUpdate();
            }
            
            yield return nodeInfo;
        }
        
        protected override bool CreateNode(INodeInfo nodeInfo, INode nodeHost)
        {
            // Will never get called.
            return true;
        }
        
        protected override bool DeleteNode(INodeInfo nodeInfo, INode nodeHost)
        {
            // Will never get called.
            return true;
        }
        
        //get the dtd string
        private void LoadDTD()
        {
            var path = Shell.CallerPath.ConcatPath(@"..");
            path = Path.GetFullPath(path);
            var files = Directory.GetFiles(path, "*.dtd");
            
            if (files.Length>0)
                using (StreamReader sr = new StreamReader(files[0]))
            {
                //add the DOCTYPE definition to place the DTD inline
                FDTD = sr.ReadLine();
                FDTD += @"<!DOCTYPE PATCH [";
                FDTD += sr.ReadToEnd();
                FDTD += @"]>";
            }
        }
        
        protected override void DoAddFile(string filename)
        {
            if (!filename.Contains("~temp"))
                base.DoAddFile(filename);
        }
        
        protected override void DoRemoveFile(string filename)
        {
            if (!filename.Contains("~temp"))
                base.DoRemoveFile(filename);
        }
    }
}
