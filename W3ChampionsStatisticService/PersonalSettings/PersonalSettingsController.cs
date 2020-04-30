﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using W3ChampionsStatisticService.Authorization;
using W3ChampionsStatisticService.PlayerProfiles;
using W3ChampionsStatisticService.Ports;

namespace W3ChampionsStatisticService.PersonalSettings
{
    [ApiController]
    [Route("api/personal-settings")]
    public class PersonalSettingsController : ControllerBase
    {
        private readonly IBlizzardAuthenticationService _authenticationService;
        private readonly IPersonalSettingsRepository _personalSettingsRepository;
        private readonly IPlayerRepository _playerRepository;

        public PersonalSettingsController(
            IBlizzardAuthenticationService authenticationService,
            IPersonalSettingsRepository personalSettingsRepository,
            IPlayerRepository playerRepository)
        {
            _authenticationService = authenticationService;
            _personalSettingsRepository = personalSettingsRepository;
            _playerRepository = playerRepository;
        }

        [HttpGet("{battleTag}")]
        public async Task<IActionResult> GetPersonalSetting(string battleTag)
        {
            var setting = await _personalSettingsRepository.Load(battleTag);
            if (setting == null)
            {
                var player = await _playerRepository.Load(battleTag);
                return Ok(new PersonalSetting(battleTag) { Players = new List<PlayerProfile> { player }});
            }
            return Ok(setting);
        }

        [HttpPut("{battleTag}/profile-message")]
        public async Task<IActionResult> SetProfileMessage(
            string battleTag,
            [FromQuery] string authentication,
            [FromBody] ProfileCommand command)
        {
            var userInfo = await _authenticationService.GetUser(authentication);
            if (userInfo == null || !battleTag.StartsWith(userInfo.battletag))
            {
                return Unauthorized("Sorry H4ckerb0i");
            }

            var setting = await _personalSettingsRepository.Load(battleTag) ?? new PersonalSetting(battleTag);
            setting.ProfileMessage = command.Value;
            await _personalSettingsRepository.Save(setting);

            return Ok();
        }

        [HttpPut("{battleTag}/home-page")]
        public async Task<IActionResult> SetHomePage(
            string battleTag,
            [FromQuery] string authentication,
            [FromBody] ProfileCommand command)
        {
            var userInfo = await _authenticationService.GetUser(authentication);
            if (userInfo == null || !battleTag.StartsWith(userInfo.battletag))
            {
                return Unauthorized("Sorry H4ckerb0i");
            }

            var setting = await _personalSettingsRepository.Load(battleTag) ?? new PersonalSetting(battleTag);
            setting.HomePage = command.Value;
            await _personalSettingsRepository.Save(setting);

            return Ok();
        }

        [HttpPut("{battleTag}/profile-picture")]
        public async Task<IActionResult> SetProfilePicture(
            string battleTag,
            [FromQuery] string authentication,
            [FromBody] SetPictureCommand command)
        {
            var userInfo = await _authenticationService.GetUser(authentication);
            if (userInfo == null || !battleTag.StartsWith(userInfo.battletag))
            {
                return Unauthorized("Sorry H4ckerb0i");
            }

            var setting = await _personalSettingsRepository.Load(battleTag) ?? new PersonalSetting(battleTag);

            var result = setting.SetProfilePicture(command.Race, command.PictureId);
            if (!result) return BadRequest();

            await _personalSettingsRepository.Save(setting);

            return Ok();
        }
    }
}