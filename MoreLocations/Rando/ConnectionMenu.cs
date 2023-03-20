﻿using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using MoreLocations.Rando.Settings;
using MoreLocations.Rando.Settings.Presets;
using RandomizerMod;
using RandomizerMod.Menu;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace MoreLocations.Rando
{
    internal class ConnectionMenu
    {
        internal static ConnectionMenu? Instance { get; private set; }

        private SmallButton rootButton;
        private MenuPage rootPage;
        private MenuElementFactory<RandomizerSettings> rootMef;
        private ToggleButton enableToggle;

        // misc page
        private SmallButton jumpToMiscPage;
        private MenuElementFactory<MiscLocationSettings> miscLocationMef;

        // lemm page
        private SmallButton jumpToLemmPage;
        private MenuElementFactory<LemmShopSettings> lemmShopRootMef;
        private MenuElementFactory<RelicGeoSettings> relicGeoSettingsMef;
        private MenuElementFactory<RelicCostSettings> relicCostSettingsMef;

        // junk shop page


        public static void Hook()
        {
            RandomizerMenuAPI.AddMenuPage(ConstructMenu, HandleButton);
            MenuChangerMod.OnExitMainMenu += () => Instance = null;
        }

        private static bool HandleButton(MenuPage landingPage, out SmallButton button)
        {
            button = Instance!.rootButton;
            return true;
        }

        private static void ConstructMenu(MenuPage connectionPage)
        {
            Instance = new ConnectionMenu(connectionPage);
        }

        public ConnectionMenu(MenuPage connectionPage)
        {
            rootPage = new MenuPage("MoreLocations", connectionPage);

            MenuLabel header = new(rootPage, "MoreLocations");
            header.MoveTo(SpaceParameters.TOP_CENTER);

            rootMef = new MenuElementFactory<RandomizerSettings>(rootPage, RandoInterop.Settings);
            enableToggle = (ToggleButton) rootMef.ElementLookup[nameof(RandomizerSettings.Enabled)];

            jumpToMiscPage = new(rootPage, "Miscellaneous Locations");
            jumpToMiscPage.AddHideAndShowEvent(CreateMiscPage());

            jumpToLemmPage = new(rootPage, "Lemm Shop");
            jumpToLemmPage.AddHideAndShowEvent(CreateLemmPage());

            VerticalItemPanel vip = new(rootPage, SpaceParameters.TOP_CENTER_UNDER_TITLE, SpaceParameters.VSPACE_SMALL, true, 
                enableToggle,
                jumpToMiscPage,
                jumpToLemmPage);

            rootButton = new(connectionPage, Localization.Localize("MoreLocations"));
            rootButton.AddHideAndShowEvent(connectionPage, rootPage);
            connectionPage.BeforeShow += SetTopLevelButtonColor(rootButton, () => RandoInterop.Settings.Enabled);
        }

        [MemberNotNull(nameof(miscLocationMef))]
        private MenuPage CreateMiscPage()
        {
            MenuPage miscPage = new("Miscellaneous Locations", rootPage);

            MenuLabel header = new(miscPage, "MoreLocations - Misc. Locations");
            header.MoveTo(SpaceParameters.TOP_CENTER);

            miscLocationMef = new(miscPage, RandoInterop.Settings.MiscLocationSettings);

            VerticalItemPanel vip = new(miscPage, SpaceParameters.TOP_CENTER_UNDER_TITLE, SpaceParameters.VSPACE_SMALL,
                true, miscLocationMef.Elements);

            rootPage.BeforeShow += SetTopLevelButtonColor(jumpToMiscPage, () => RandoInterop.Settings.MiscLocationSettings.Any);

            return miscPage;
        }

        [MemberNotNull(nameof(lemmShopRootMef), nameof(relicGeoSettingsMef), nameof(relicCostSettingsMef))]
        private MenuPage CreateLemmPage()
        {
            MenuPage lemmPage = new("Lemm Shop", rootPage);

            MenuLabel header = new(lemmPage, "MoreLocations - Lemm");
            header.MoveTo(SpaceParameters.TOP_CENTER);

            lemmShopRootMef = new(lemmPage, RandoInterop.Settings.LemmShopSettings);

            relicGeoSettingsMef = new(lemmPage, RandoInterop.Settings.LemmShopSettings.GeoSettings);
            MenuPreset<RelicGeoSettings> geoSettingsPreset = new(lemmPage, "Relic Geo Items",
                RelicGeoPresets.Presets, RandoInterop.Settings.LemmShopSettings.GeoSettings,
                _ => "",
                relicGeoSettingsMef);
            GridItemPanel geoSettingsHorizontalGrid = new(lemmPage, Vector2.zero, 4, 
                0f, SpaceParameters.HSPACE_SMALL, false, 
                relicGeoSettingsMef.Elements);

            relicCostSettingsMef = new(lemmPage, RandoInterop.Settings.LemmShopSettings.CostSettings);
            MenuPreset<RelicCostSettings> costSettingsPreset = new(lemmPage, "Relic Costs",
                RelicCostPresets.Presets, RandoInterop.Settings.LemmShopSettings.CostSettings,
                _ => "",
                relicCostSettingsMef);
            GridItemPanel costSettingsHorizontalGrid = new(lemmPage, Vector2.zero, 3,
                SpaceParameters.VSPACE_MEDIUM, SpaceParameters.HSPACE_SMALL, false,
                relicCostSettingsMef.Elements);

            IMenuElement[] vipElements = lemmShopRootMef.Elements.OfType<IMenuElement>()
                .Append(geoSettingsPreset)
                .Append(geoSettingsHorizontalGrid)
                .Append(costSettingsPreset)
                .Append(costSettingsHorizontalGrid)
                .ToArray();

            VerticalItemPanel vip = new(lemmPage, SpaceParameters.TOP_CENTER_UNDER_TITLE, SpaceParameters.VSPACE_MEDIUM,
                true, vipElements);

            rootPage.BeforeShow += SetTopLevelButtonColor(jumpToLemmPage, () => RandoInterop.Settings.LemmShopSettings.Enabled);

            return lemmPage;
        }

        private Action SetTopLevelButtonColor(SmallButton target, Func<bool> condition)
        {
            return () =>
            {
                target.Text.color = condition() ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            };
        }
    }
}
