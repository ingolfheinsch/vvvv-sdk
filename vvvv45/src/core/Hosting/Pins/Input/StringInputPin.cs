﻿using System;
using System.Runtime.InteropServices;
using System.IO;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class StringInputPin : DiffPin<string>
	{
		protected IStringIn FStringIn;
		protected bool FIsPath;
		
		public StringInputPin(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateStringInput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FStringIn);
			FStringIn.SetSubType2(attribute.DefaultString, attribute.MaxChars, attribute.FileMask, (TStringType)attribute.StringType);
			
			FIsPath = (attribute.StringType == StringType.Directory) || (attribute.StringType == StringType.Filename);
			
			FBuffer = new string[1];
			
			base.InitializeInternalPin(FStringIn);
		}
		
		protected override bool IsInternalPinChanged
		{
			get
			{
				return FStringIn.PinIsChanged;
			}
		}
		
		protected string GetFullPath(string path)
		{
			if(Path.IsPathRooted(path)) return path;
			
			string patchPath;
			FHost.GetHostPath(out patchPath);
			
			try
			{
				patchPath = Path.GetDirectoryName(patchPath);
				path = Path.GetFullPath(Path.Combine(patchPath, path));
			}
			catch (Exception e)
			{
				FLogger.Log(LogType.Error, e.Message);
			}
			
			return path;
		}
		
		unsafe protected override void DoUpdate()
		{
			SliceCount = FStringIn.SliceCount;
			
			for (int i = 0; i < FSliceCount; i++)
			{
				string value;
				FStringIn.GetString(i, out value);
				var s = value == null ? "" : value;
				FBuffer[i] = FIsPath ? GetFullPath(s) : s;
			}
		}
	}
}
