﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Carter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino.PlugIns;

namespace compute.geometry
{
    public class FixedEndPointsModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("", HomePage);
            app.MapGet("version", GetVersion);
            app.MapGet("servertime", ServerTime);
            app.MapGet("plugins/rhino/installed", GetInstalledPluginsRhino);
            app.MapGet("plugins/gh/installed", GetInstalledPluginsGrasshopper);
            app.MapGet("speckle", Speckle);
        }

        private static async Task Speckle(HttpContext ctx)
        {
            var values = new Dictionary<string, string>
            {
                { "rhino", Rhino.RhinoApp.Version.ToString() },
                { "compute", Assembly.GetExecutingAssembly().GetName().Version.ToString() }
            };
            string git_sha = null; // appveyor will replace this
            values.Add("git_sha", git_sha);
            values.Add("speckle", "speckle is the best");
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(values);
        }

        private static void HomePage(HttpContext context)
        {
            context.Response.Redirect("https://www.rhino3d.com/compute");
        }

        private static async Task GetVersion(HttpContext ctx)
        {
            var values = new Dictionary<string, string>
            {
                { "rhino", Rhino.RhinoApp.Version.ToString() },
                { "compute", Assembly.GetExecutingAssembly().GetName().Version.ToString() }
            };
            string git_sha = null; // appveyor will replace this
            values.Add("git_sha", git_sha);

            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(values);
        }

        private static async Task ServerTime(HttpContext ctx)
        {
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(DateTime.UtcNow);
        }

        private static async Task GetInstalledPluginsRhino(HttpContext ctx)
        {
            var rhPluginInfo = new SortedDictionary<string, string>();
            foreach (var k in Rhino.PlugIns.PlugIn.GetInstalledPlugIns().Keys)
            {
                var info = Rhino.PlugIns.PlugIn.GetPlugInInfo(k);
                //Could also use: info.IsLoaded
                if (info != null && !info.ShipsWithRhino && !rhPluginInfo.ContainsKey(info.Name))
                {
                    rhPluginInfo.Add(info.Name, info.Version);
                }
            }

            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(rhPluginInfo);
        }

        private static async Task GetInstalledPluginsGrasshopper(HttpContext ctx)
        {
            var ghPluginInfo = new SortedDictionary<string, string>();
            foreach (var obj in Grasshopper.Instances.ComponentServer.ObjectProxies.Where(o => o != null))
            {
                var asm = Grasshopper.Instances.ComponentServer.FindAssemblyByObject(obj.Guid);
                if (asm != null && !string.IsNullOrEmpty(asm.Name) && !asm.IsCoreLibrary && !ghPluginInfo.ContainsKey(asm.Name))
                {
                    var version = (string.IsNullOrEmpty(asm.Version)) ? asm.Assembly.GetName().Version.ToString() : asm.Version;
                    ghPluginInfo.Add(asm.Name, version);
                }
            }
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(ghPluginInfo);
        }
    }
}
