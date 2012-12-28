using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace RuntimeCodeCompiler
{
    /// <summary>
    /// Compiles a given string into an assembly at runtime 
    /// </summary>
    /// <remarks>
    /// Modified from: http://stackoverflow.com/a/3024112/670028
    /// </remarks>
    public static class RuntimeCodeCompiler
    {
        #region Private Variables

        private static readonly Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();

        #endregion Private Variables

        #region Constructor

        static RuntimeCodeCompiler()
        {
            AppDomain.CurrentDomain.AssemblyLoad += ( sender, e ) =>
            {
                Assemblies[ e.LoadedAssembly.FullName ] = e.LoadedAssembly;
            };

            AppDomain.CurrentDomain.AssemblyResolve += ( sender, e ) =>
            {
                Assembly assembly = null;
                
                Assemblies.TryGetValue( e.Name, out assembly );

                if ( assembly == null )
                {
                    foreach ( var a in Assemblies )
                    {
                        if ( a.Key.StartsWith( e.Name ) )
                        {
                            return a.Value;
                        }
                    }
                }

                return assembly;
            };
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Compiles code into an in memory assembly
        /// </summary>
        /// <param name="code">C# code</param>
        /// <param name="assemblyName">Assembly name to create</param>
        /// <returns>Runtime-compiled assembly</returns>
        public static Assembly Compile( string code, string assemblyName )
        {
            if ( string.IsNullOrEmpty( code ) || string.IsNullOrEmpty( assemblyName ) )
            {
                return null;
            }

            var provider = new CSharpCodeProvider();

            var compilerparams = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                OutputAssembly = assemblyName
            };

            foreach ( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() )
            {
                try
                {
                    string location = assembly.Location;

                    if ( !String.IsNullOrEmpty( location ) )
                    {
                        compilerparams.ReferencedAssemblies.Add( location );
                    }
                }
                catch
                { 
                }
            }

            CompilerResults results = provider.CompileAssemblyFromSource( compilerparams, code );

            if ( results.Errors.HasErrors )
            {
                var errors = new StringBuilder( "Compiler Errors :\r\n" );

                foreach ( CompilerError error in results.Errors )
                {
                    errors.AppendFormat( "Line {0},{1}\t: {2}\n",
                           error.Line, error.Column, error.ErrorText );
                }

                throw new Exception( errors.ToString() );
            }

            AppDomain.CurrentDomain.Load( results.CompiledAssembly.GetName() );

            return results.CompiledAssembly;
        }

        #endregion Public Methods
    }
}
