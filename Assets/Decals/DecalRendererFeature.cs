using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class DecalRendererFeature : ScriptableRendererFeature
{
    class DecalRenderPass : ScriptableRenderPass
    {
        private DecalSettings settings;
        private ProfilingSampler profilingSampler;

        public DecalRenderPass(string profilingTag)
        {
            profilingSampler = new ProfilingSampler(profilingTag);
        }

        public void Setup(DecalSettings settings)
        {
            this.settings = settings;

            renderPassEvent = settings.RenderPassEvent;
        }


        // This class stores the data needed by the RenderGraph pass.
        // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
        private class PassData
        {
            public Material DecalMaterial;
            public Mesh DecalMesh;
            public List<Matrix4x4> DecalMatrices;
            public List<Vector4> DecalColors;
        }


        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (DecalManager.SharedInstance == null || DecalManager.SharedInstance.ActiveDecals.Count == 0) return;

            const string passName = "Custom Render Pass";

            // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData, profilingSampler))
            {
                // Use this scope to set the required inputs and outputs of the pass and to
                // setup the passData with the required properties needed at pass execution time.

                // Make use of frameData to access resources and camera data through the dedicated containers.
                // Eg:
                // UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                passData.DecalMaterial = settings.DecalMaterial;
                passData.DecalMesh = settings.DecalMesh;
                passData.DecalMatrices = new List<Matrix4x4>(DecalManager.SharedInstance.ActiveDecals.Count);
                passData.DecalColors = new List<Vector4>(DecalManager.SharedInstance.ActiveDecals.Count);

                foreach(var decal in DecalManager.SharedInstance.ActiveDecals)
                {
                    passData.DecalMatrices.Add(Matrix4x4.TRS(decal.Position, decal.Rotation, decal.Size));
                    passData.DecalColors.Add((Vector4)decal.Color);
                }

                // Setup pass inputs and outputs through the builder interface.
                // Eg:
                // builder.UseTexture(sourceTexture);
                // TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraData.cameraTargetDescriptor, "Destination Texture", false);

                // This sets the render target of the pass to the active color texture. Change it to your own render target as needed.
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);

                // Assigns the ExecutePass function to the render pass delegate. This will be called by the render graph when executing the pass.
                builder.SetRenderFunc<PassData>(ExecutePass);
            }
        }


        // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
        // It is used to execute draw commands.
        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            var mpb = new MaterialPropertyBlock();
            mpb.SetVectorArray("_SplatColor", data.DecalColors);
            
            context.cmd.DrawMeshInstanced(
                data.DecalMesh,
                0,
                data.DecalMaterial,
                0,
                data.DecalMatrices.ToArray(),
                data.DecalMatrices.Count,
                mpb
                );
        }
    }

    [System.Serializable]
    public class DecalSettings
    {
        public Material DecalMaterial;
        public Mesh DecalMesh;
        public RenderPassEvent RenderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
    }

    public DecalSettings Settings = new DecalSettings();
    DecalRenderPass decalPass;

    /// <inheritdoc/>
    public override void Create()
    {
        decalPass = new DecalRenderPass("2D Decal Pass");
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (Settings.DecalMaterial == null || Settings.DecalMesh == null) return;

        decalPass.Setup(Settings);

        renderer.EnqueuePass(decalPass);
    }
}


