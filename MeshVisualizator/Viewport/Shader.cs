﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MeshVisualizator
{
   public class ShaderProgram : IDisposable
   {
      ShaderType[] types;
      string[] shader_files;
      int shaderProgram = -1;
      List<int> shaders;
      private readonly Dictionary<string, int> uniformLocations;
      public ShaderProgram(string[] shader_filepathes, ShaderType[] types) 
      {
         this.shader_files = shader_filepathes;
         this.types = types;
         uniformLocations = new Dictionary<string, int>();
         shaderProgram = GL.CreateProgram();
         shaders = new List<int>();
      }
      private int MakeShader(string shader_filepath, ShaderType type)
      {
         int shader = GL.CreateShader(type);
         using (StreamReader sr = new(@"../../../" + shader_filepath))
         {
            string shader_text = sr.ReadToEnd();
            GL.ShaderSource(shader, shader_text);
         }

         GL.CompileShader(shader);
         int status;
         GL.GetShader(shader, ShaderParameter.CompileStatus, out status);

         if (status == 0)
         {
            string info;
            GL.GetShaderInfoLog(shader, out info);
            //TODO: logger
            throw new Exception(info);
         }

         GL.AttachShader(shaderProgram, shader);
         shaders.Add(shader);
         return status;
      }
      ~ShaderProgram()
      {
         //GL.DeleteProgram(shaderProgram);
      }
      public void UseShaders() => GL.UseProgram(shaderProgram);
      public void LinkShaders()
      {
         bool success = true;
         for (int i = 0; i < shader_files.Length && success; i++)
            success = MakeShader(shader_files[i], types[i]) != 0;

         if (!success)
            throw new Exception("Some shader did not compile!");

         GL.LinkProgram(shaderProgram);

         foreach (int sh in shaders)
         {
            GL.DetachShader(shaderProgram, sh);
            GL.DeleteShader(sh);
         }
         int status;
         GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out status);

         if (status == 0)
         {
            string info;
            GL.GetProgramInfoLog(shaderProgram, out info);
            //TODO: logger
            //throw new Exception(info);
            Debug.WriteLine(info);
         }

         GL.GetProgram(shaderProgram, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
         for (var i = 0; i < numberOfUniforms; i++)
         {
            var key = GL.GetActiveUniform(shaderProgram, i, out _, out _);
            var location = GL.GetUniformLocation(shaderProgram, key);
            uniformLocations.Add(key, location);
         }
      }
      public void SetBool(string name, bool value)
      {
         UseShaders();
         GL.Uniform1(GL.GetUniformLocation(shaderProgram, name), Convert.ToInt32(value));
      }
      public void SetInt(string name, int value)
      {
         UseShaders();
         GL.Uniform1(GL.GetUniformLocation(shaderProgram, name), value);
      }
      public void SetFloat(string name, float value)
      {
         UseShaders();
         GL.Uniform1(GL.GetUniformLocation(shaderProgram, name), value);
      }
      public void SetMatrix4(string name, Matrix4 mat4)
      {
         UseShaders();
         GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, name), false, ref mat4);
      }

      public int GetAttribLocation(string name) =>
         GL.GetAttribLocation(shaderProgram, name);


      private bool disposedValue = false;
      protected virtual void Dispose(bool disposing)
      {
         if (!disposedValue)
         {
            GL.DeleteProgram(shaderProgram);
            disposedValue = true;
         }
      }  
      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }
   }
}