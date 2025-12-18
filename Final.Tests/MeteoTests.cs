using Final.DataService.Repositories.Interfaces;
using Final.Models;
using Final.Models.Weatherbit;
using Final.Services.Interfaces;
using Final.ViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Final.Tests
{
    public class MeteoViewModelTest
    {
        private readonly Mock<IDialogueService> _dialServiceMock = new Mock<IDialogueService>();
        private readonly Mock<IRegionRepository> _regionRepoMock = new Mock<IRegionRepository>();
        private readonly Mock<IWeatherService> _weatherServiceMock = new Mock<IWeatherService>();

        public MeteoViewModelTest()
        {
            _dialServiceMock.Setup(ds => ds.AfficherMessageErreur(It.IsAny<string>(), It.IsAny<string>()));
            _dialServiceMock.Setup(ds => ds.AfficherMessageInfo(It.IsAny<string>(), It.IsAny<string>()));
            _dialServiceMock.Setup(ds => ds.AfficherMessageAvertissement(It.IsAny<string>(), It.IsAny<string>()));
            _dialServiceMock.Setup(ds => ds.PoserQuestion(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            _weatherServiceMock
                .Setup(ws => ws.Get7DaysAsync(It.IsAny<Region>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<WeatherDay>());
        }

        [Fact]
        public void Constructeur_ShouldBeValid()
        {
            // Préparation
            _regionRepoMock.Setup(rr => rr.GetAll()).Returns(ListeRegionsAttendues());

            // Exécution
            MeteoViewModel vm = new MeteoViewModel(
                _regionRepoMock.Object,
                _weatherServiceMock.Object,
                _dialServiceMock.Object
            );

            // Affirmation
            Assert.Equal(3, vm.Regions.Count);
            Assert.Empty(vm.Previsions);
            Assert.True(vm.ModeAjout);
            Assert.Null(vm.RegionSelectionnee);
            Assert.Equal("", vm.Nom);
            Assert.Equal("", vm.Latitude);
            Assert.Equal("", vm.Longitude);

            _regionRepoMock.Verify(rr => rr.GetAll(), Times.Once);
        }

        [Fact]
        public void NouvelleRegion_ShouldResetFields()
        {
            // Préparation
            _regionRepoMock.Setup(rr => rr.GetAll()).Returns(ListeRegionsAttendues());
            MeteoViewModel vm = new MeteoViewModel(_regionRepoMock.Object, _weatherServiceMock.Object, _dialServiceMock.Object);

            vm.Nom = "X";
            vm.Latitude = "1";
            vm.Longitude = "2";
            vm.ModeAjout = false;

            // Exécution
            vm.CommandeNouvelleRegion.Execute(null);

            // Affirmation
            Assert.Null(vm.RegionSelectionnee);
            Assert.True(vm.ModeAjout);
            Assert.Equal("", vm.Nom);
            Assert.Equal("", vm.Latitude);
            Assert.Equal("", vm.Longitude);
        }

        [Fact]
        public async Task Ajouter_RegionValide_ShouldCallAddAsync_AndAddToRegions()
        {
            // Préparation
            _regionRepoMock.Setup(rr => rr.GetAll()).Returns(ListeRegionsAttendues());

            _regionRepoMock
                .Setup(rr => rr.AddAsync(It.IsAny<Region>()))
                .ReturnsAsync((Region r) => r);

            MeteoViewModel vm = new MeteoViewModel(_regionRepoMock.Object, _weatherServiceMock.Object, _dialServiceMock.Object);

            vm.Nom = "Quebec2";
            vm.Latitude = "46.8131";
            vm.Longitude = "-71.2128";

            // Exécution (AsyncCommand = async void)
            await vm.AjouterAsync(null);

            // Affirmation
            _regionRepoMock.Verify(rr => rr.AddAsync(It.Is<Region>(r =>
                r.Nom == "Quebec2" &&
                Math.Abs(r.Latitude - 46.8131) < 0.0001 &&
                Math.Abs(r.Longitude - (-71.2128)) < 0.0001
            )), Times.Once);

            Assert.Contains(vm.Regions, r => r.Nom == "Quebec2");

            // reset champs après ajout
            Assert.Equal("", vm.Nom);
            Assert.Equal("", vm.Latitude);
            Assert.Equal("", vm.Longitude);

            _dialServiceMock.Verify(ds => ds.AfficherMessageInfo(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Ajouter_LatLonInvalides_ShouldNotCallAddAsync_AndShowWarning()
        {
            // Préparation
            _regionRepoMock.Setup(rr => rr.GetAll()).Returns(new List<Region>());
            MeteoViewModel vm = new MeteoViewModel(_regionRepoMock.Object, _weatherServiceMock.Object, _dialServiceMock.Object);

            vm.Nom = "Test";
            vm.Latitude = "abc";
            vm.Longitude = "123";

            // Exécution
            await vm.AjouterAsync(null);


            // Affirmation
            _regionRepoMock.Verify(rr => rr.AddAsync(It.IsAny<Region>()), Times.Never);
            _dialServiceMock.Verify(ds => ds.AfficherMessageAvertissement(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.Empty(vm.Regions);
        }

        [Fact]
        public async Task Ajouter_NomDejaExistant_ShouldNotCallAddAsync_AndShowWarning()
        {
            // Préparation
            _regionRepoMock.Setup(rr => rr.GetAll()).Returns(ListeRegionsAttendues());
            MeteoViewModel vm = new MeteoViewModel(_regionRepoMock.Object, _weatherServiceMock.Object, _dialServiceMock.Object);

            vm.Nom = "  shawinigan  "; // trim + ignore case
            vm.Latitude = "46.56";
            vm.Longitude = "-72.73";

            // Exécution
            await vm.AjouterAsync(null);

            // Affirmation
            _regionRepoMock.Verify(rr => rr.AddAsync(It.IsAny<Region>()), Times.Never);
            _dialServiceMock.Verify(ds => ds.AfficherMessageAvertissement(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.Equal(3, vm.Regions.Count);
        }

        [Fact]
        public async Task Ajouter_DbUpdateException_Unique_ShouldShowWarning_AndNotAdd()
        {
            // Préparation
            _regionRepoMock.Setup(rr => rr.GetAll()).Returns(new List<Region>());

            _regionRepoMock
                .Setup(rr => rr.AddAsync(It.IsAny<Region>()))
                .ThrowsAsync(new DbUpdateException("DbUpdateException", new Exception("UNIQUE constraint failed")));

            MeteoViewModel vm = new MeteoViewModel(_regionRepoMock.Object, _weatherServiceMock.Object, _dialServiceMock.Object);

            vm.Nom = "Montreal";
            vm.Latitude = "45.5";
            vm.Longitude = "-73.58";

            // Exécution
            await vm.AjouterAsync(null);

            // Affirmation
            Assert.Empty(vm.Regions);
            _dialServiceMock.Verify(ds => ds.AfficherMessageAvertissement(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Supprimer_SeedId1_ShouldNotDelete_AndShowWarning()
        {
            Properties.Settings.Default.tokenWeatherbit = "fake-token";
            Properties.Settings.Default.langue = "fr-CA";

            // Préparation
            var regions = ListeRegionsAttendues();
            _regionRepoMock.Setup(rr => rr.GetAll()).Returns(regions);

            _weatherServiceMock
                .Setup(ws => ws.Get7DaysAsync(It.IsAny<Region>(), "fake-token", "fr"))
                .ReturnsAsync(new List<WeatherDay>());

            MeteoViewModel vm = new MeteoViewModel(
                _regionRepoMock.Object,
                _weatherServiceMock.Object,
                _dialServiceMock.Object
            );

            vm.RegionSelectionnee = vm.Regions.First(r => r.Id == 1);

            // Exécution
            await vm.SupprimerAsync(null);

            // Affirmation
            _regionRepoMock.Verify(rr => rr.DeleteAsync(It.IsAny<Region>()), Times.Never);

            _dialServiceMock.Verify(ds => ds.AfficherMessageAvertissement(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            Assert.Contains(vm.Regions, r => r.Id == 1);
        }

        [Fact]
        public async Task Supprimer_UserSaysNo_ShouldNotDelete()
        {
            // Préparation
            _regionRepoMock.Setup(rr => rr.GetAll()).Returns(ListeRegionsAttendues());

            _dialServiceMock
                .Setup(ds => ds.PoserQuestion(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            MeteoViewModel vm = new MeteoViewModel(_regionRepoMock.Object, _weatherServiceMock.Object, _dialServiceMock.Object);

            vm.RegionSelectionnee = vm.Regions.First(r => r.Id == 2);

            // Exécution
            await vm.SupprimerAsync(null);

            // Affirmation
            _regionRepoMock.Verify(rr => rr.DeleteAsync(It.IsAny<Region>()), Times.Never);
            Assert.Contains(vm.Regions, r => r.Id == 2);
        }

        [Fact]
        public async Task Supprimer_UserSaysYes_ShouldDelete_AndRemoveFromRegions()
        {
            // Préparation
            _regionRepoMock.Setup(rr => rr.GetAll()).Returns(ListeRegionsAttendues());

            _dialServiceMock
                .Setup(ds => ds.PoserQuestion(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            _regionRepoMock
                .Setup(rr => rr.DeleteAsync(It.IsAny<Region>()))
                .ReturnsAsync(true);

            MeteoViewModel vm = new MeteoViewModel(_regionRepoMock.Object, _weatherServiceMock.Object, _dialServiceMock.Object);

            vm.RegionSelectionnee = vm.Regions.First(r => r.Id == 2);

            // Exécution
            await vm.SupprimerAsync(null);

            // Affirmation
            _regionRepoMock.Verify(rr => rr.DeleteAsync(It.Is<Region>(r => r.Id == 2)), Times.Once);
            Assert.DoesNotContain(vm.Regions, r => r.Id == 2);
            _dialServiceMock.Verify(ds => ds.AfficherMessageInfo(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        

        

        private List<Region> ListeRegionsAttendues()
        {
            return new List<Region>()
            {
                new Region { Id = 1, Nom = "Shawinigan", Latitude = 46.56984172477484, Longitude = -72.73811285651442 },
                new Region { Id = 2, Nom = "Montreal",   Latitude = 45.50346478409937, Longitude = -73.5884530315365 },
                new Region { Id = 3, Nom = "Quebec",     Latitude = 46.81313351998989, Longitude = -71.21282419393238 },
            };
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Ajouter_NomNullOuBlank_ShouldNotAdd_AndShowWarning(string nomInvalide)
        {
            // Préparation
            _regionRepoMock.Setup(rr => rr.GetAll()).Returns(new List<Region>());

            MeteoViewModel vm = new MeteoViewModel(
                _regionRepoMock.Object,
                _weatherServiceMock.Object,
                _dialServiceMock.Object
            );

            vm.Nom = nomInvalide;
            vm.Latitude = "45.5";
            vm.Longitude = "-73.58";

            // Exécution
            await vm.AjouterAsync(null);

            // Affirmation
            _regionRepoMock.Verify(rr => rr.AddAsync(It.IsAny<Region>()), Times.Never);

            _dialServiceMock.Verify(ds =>
                ds.AfficherMessageAvertissement(It.IsAny<string>(), It.IsAny<string>()),
                Times.Once
            );

            Assert.Empty(vm.Regions);
        }

    }
}
