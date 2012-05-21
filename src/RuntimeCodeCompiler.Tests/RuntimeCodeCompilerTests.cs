using System;
using System.Linq;
using NUnit.Framework;

namespace RuntimeCodeCompiler.Tests
{
    [TestFixture]
    public class RuntimeCodeCompilerTests
    {
        private const string ValidCSharpCode =
@"
using System;

namespace RuntimeCodeCompiler.Tests
{
    public class RuntimeCompiledHelloWorld : IRuntimeCompiledHelloWorld
    {
        public void Hello()
        {
            Console.WriteLine( ""Hello World!"" );
        }
    }
}";

        [Test]
        public void Compile_Should_Successfully_Compile_A_Valid_String_Of_CSharp_Code()
        {
            var assembly = RuntimeCodeCompiler.Compile( ValidCSharpCode, "RuntimeCompiled" );

            Assert.NotNull( assembly );
        }

        [Test, Explicit]
        public void Compile_Should_Throw_An_Exception_When_Compiling_An_Invalid_String_Of_CSharp_Code()
        {
            string invalidCode = ValidCSharpCode.Replace( ";", "" );

            Assert.That( () => RuntimeCodeCompiler.Compile( invalidCode, "RuntimeCompiled" ),
                Throws.Exception.Message.Contains( "; expected" ) );
        }

        [Test]
        public void The_Compiled_Assembly_Should_Be_Callable_Via_The_Interface()
        {
            var assembly = RuntimeCodeCompiler.Compile( ValidCSharpCode, "RuntimeCompiled" );

            var runtimeCompiledHelloWorldType = assembly.GetType( "RuntimeCodeCompiler.Tests.RuntimeCompiledHelloWorld" );

            var runtimeCompiledHelloWorld = Activator.CreateInstance( runtimeCompiledHelloWorldType ) as IRuntimeCompiledHelloWorld;

            runtimeCompiledHelloWorld.Hello();
        }

        [Test]
        public void The_Compiled_Assembly_Should_Be_Callable_Via_Reflection()
        {
            var assembly = RuntimeCodeCompiler.Compile( ValidCSharpCode, "RuntimeCompiled" );

            var runtimeCompiledHelloWorldType = assembly.GetType( "RuntimeCodeCompiler.Tests.RuntimeCompiledHelloWorld" );

            var runtimeCompiledHelloWorld = Activator.CreateInstance( runtimeCompiledHelloWorldType );

            runtimeCompiledHelloWorldType.GetMethod( "Hello" ).Invoke( runtimeCompiledHelloWorld, null );
        }

        [Test]
        public void Compile_Should_Have_Already_Loaded_The_Assembly_Into_The_Current_AppDomain()
        {
            var assembly = RuntimeCodeCompiler.Compile( ValidCSharpCode, "RuntimeCompiled" );

            var runtimeCompiledAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .Where( x => x.FullName.Contains( "RuntimeCompiled" ) )
                .SingleOrDefault();

            Assert.NotNull( runtimeCompiledAssembly );
        }
    }

    /// <summary>
    /// Interface for the runtime-compiled code
    /// </summary>
    public interface IRuntimeCompiledHelloWorld
    {
        void Hello();
    }
}
