﻿/*
 * Erstellt mit SharpDevelop.
 * Benutzer: Elias
 * Datum: 21.08.2009
 * Zeit: 23:20
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Runtime.InteropServices;

using ColladaSlimDX.ColladaDocument;
using ColladaSlimDX.ColladaModel;
using ColladaSlimDX.Utils;
using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
    [PluginInfo(Name = "Camera",
                Category = "EX9.Geometry",
                Version = "Collada",
                Author = "vvvv group",
                Help = "Returns view and projection matrix of selected cameras.",
                Tags = "collada, dae")]
    public class PluginColladaCamera : IPluginEvaluate
    {
        #region field declaration
        
        //the host (mandatory)
        private Model FColladaModel;
        private List<Model.InstanceCamera> FSelectedInstanceCameras;
        
        //input pin declaration
        [Input("COLLADA Model")]
        protected IDiffSpread<Model> FColladaModelIn;
        
        [Input("Time")]
        protected IDiffSpread<float> FTimeIn;
        
        [Input("Index")]
        protected IDiffSpread<int> FIndexIn;
        
        //output pin declaration
        [Output("View Matrix", Order = 1)]
        protected ISpread<Matrix> FViewMatrixOut;
        
        [Output("Projection Matrix", Order = 2)]
        protected ISpread<Matrix> FProjectionMatrixOut;
        
        protected ICOLLADALogger FColladaLogger;
        
        #endregion
        [ImportingConstructor]
        public PluginColladaCamera(ILogger logger)
        {
            FColladaLogger = new LoggerWrapper(logger);
            FSelectedInstanceCameras = new List<Model.InstanceCamera>();
        }

        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {
            COLLADAUtil.Logger = FColladaLogger;
            
            if (FColladaModelIn.IsChanged || FIndexIn.IsChanged || FTimeIn.IsChanged)
            {
                FSelectedInstanceCameras.Clear();
                
                FColladaModel = FColladaModelIn[0];
                
                if (FColladaModel == null || FColladaModel.InstanceCameras.Count == 0)
                {
                    FViewMatrixOut.SliceCount = 0;
                    FProjectionMatrixOut.SliceCount = 0;
                }
                else
                {
                    FViewMatrixOut.SliceCount = SpreadMax;
                    FProjectionMatrixOut.SliceCount = SpreadMax;
                    
                    for (int i = 0; i < FIndexIn.SliceCount; i++)
                    {
                        int index = FIndexIn[i] % FColladaModel.InstanceCameras.Count;
                        
                        var camera = FColladaModel.InstanceCameras[index];
                        var inverse = FColladaModel.ConversionMatrixInverse;
//                        var viewMatrix = camera.GetViewMatrix(FTimeIn[i]) * FColladaModel.ConversionMatrix;
                        
                        var viewMatrix = Matrix.Scaling(1f, 1f, -1f) * camera.GetViewMatrix(FTimeIn[i]) * FColladaModel.ConversionMatrix;
                        viewMatrix.Invert();
						FViewMatrixOut[i] = viewMatrix;
                        FProjectionMatrixOut[i] = camera.ProjectionMatrix;
                    }
                }
            }
        }
    }
}
