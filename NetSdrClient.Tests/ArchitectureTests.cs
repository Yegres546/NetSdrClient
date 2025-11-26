using NetArchTest.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetSdrClient.Tests
{
    [TestClass]
    public class ArchitectureTests
    {
        private const string ApplicationNamespace = "NetSdrClient";
        private const string ModelsNamespace = "NetSdrClient.Models";
        private const string ServicesNamespace = "NetSdrClient.Services";
        private const string InterfacesNamespace = "NetSdrClient.Interfaces";
        private const string InfrastructureNamespace = "NetSdrClient.Infrastructure";
        private const string UINamespace = "NetSdrClient.UI";

        [TestMethod]
        public void ServicesLayer_ShouldNotDependOnUI()
        {
            // Arrange
            var assembly = typeof(Services.SDRClient).Assembly;

            // Act
            var result = Types
                .InAssembly(assembly)
                .That()
                .ResideInNamespace(ServicesNamespace)
                .ShouldNot()
                .HaveDependencyOn(UINamespace)
                .GetResult();

            // Assert
            Assert.IsTrue(result.IsSuccessful, 
                $"Services layer should not depend on UI: {string.Join(", ", result.FailingTypes)}");
        }

        [TestMethod]
        public void Models_ShouldNotReferenceServices()
        {
            // Arrange
            var assembly = typeof(Models.SDRDevice).Assembly;

            // Act
            var result = Types
                .InAssembly(assembly)
                .That()
                .ResideInNamespace(ModelsNamespace)
                .ShouldNot()
                .HaveDependencyOn(ServicesNamespace)
                .GetResult();

            // Assert
            Assert.IsTrue(result.IsSuccessful,
                $"Models should not depend on Services: {string.Join(", ", result.FailingTypes)}");
        }

        [TestMethod]
        public void Interfaces_ShouldNotHaveDependencies()
        {
            // Arrange
            var assembly = typeof(Services.SDRClient).Assembly;

            // Act
            var result = Types
                .InAssembly(assembly)
                .That()
                .ResideInNamespace(InterfacesNamespace)
                .Should()
                .NotHaveDependencyOnAny(
                    ServicesNamespace,
                    ModelsNamespace,
                    InfrastructureNamespace,
                    UINamespace)
                .GetResult();

            // Assert
            Assert.IsTrue(result.IsSuccessful,
                $"Interfaces should not have dependencies: {string.Join(", ", result.FailingTypes)}");
        }

        [TestMethod]
        public void AllClasses_ShouldHaveNamesEndingWithService_IfInServicesNamespace()
        {
            // Arrange
            var assembly = typeof(Services.SDRClient).Assembly;

            // Act
            var result = Types
                .InAssembly(assembly)
                .That()
                .ResideInNamespace(ServicesNamespace)
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("Service")
                .Or()
                .HaveNameEndingWith("Client")
                .Or()
                .HaveNameEndingWith("Handler")
                .GetResult();

            // Assert
            Assert.IsTrue(result.IsSuccessful,
                $"Services should have proper naming: {string.Join(", ", result.FailingTypes)}");
        }

        [TestMethod]
        public void Models_ShouldBeSealed()
        {
            // Arrange
            var assembly = typeof(Models.SDRDevice).Assembly;

            // Act
            var result = Types
                .InAssembly(assembly)
                .That()
                .ResideInNamespace(ModelsNamespace)
                .And()
                .AreClasses()
                .Should()
                .BeSealed()
                .GetResult();

            // Assert
            Assert.IsTrue(result.IsSuccessful,
                $"Models should be sealed: {string.Join(", ", result.FailingTypes)}");
        }

        [TestMethod]
        public void Services_ShouldNotDependOnInfrastructureDirectly()
        {
            // Arrange
            var assembly = typeof(Services.SDRClient).Assembly;

            // Act
            var result = Types
                .InAssembly(assembly)
                .That()
                .ResideInNamespace(ServicesNamespace)
                .ShouldNot()
                .HaveDependencyOn(InfrastructureNamespace)
                .GetResult();

            // Assert
            Assert.IsTrue(result.IsSuccessful,
                $"Services should not depend directly on Infrastructure: {string.Join(", ", result.FailingTypes)}");
        }
    }
}
