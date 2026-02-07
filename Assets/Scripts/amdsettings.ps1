
Get-WmiObject -Class Win32_VideoController | Where-Object { $_.PNPDeviceID -ne $null } | ForEach-Object {
    $pnpDeviceId = $_.PNPDeviceID
    $regPath = "HKLM:\SYSTEM\CurrentControlSet\Enum\$pnpDeviceId"
    $driver = (Get-ItemProperty -Path $regPath -Name "Driver" -ErrorAction SilentlyContinue).Driver
    $classKey = "HKLM:\SYSTEM\CurrentControlSet\Control\Class\$driver"
    $providerName = (Get-ItemProperty -Path $classKey -Name "ProviderName" -ErrorAction SilentlyContinue).ProviderName
    
    if ($providerName -eq "Advanced Micro Devices, Inc.") {
        # Disable "Radeon™ Super Resolution"
        New-ItemProperty -Path $classKey -Name "KMD_RadeonUpscalingEnabled" -Value 0 -PropertyType DWord -Force

        # Disable "AMD Fluid Motion Frames 2.1"
        New-ItemProperty -Path $classKey -Name "DrvFrameGenEnabled" -Value ([byte[]](0x00,0x00,0x00,0x00)) -PropertyType Binary -Force

        # Disable "Radeon™ Anti Lag"
        New-ItemProperty -Path $classKey -Name "KMD_DeLagEnabled" -Value 0 -PropertyType DWord -Force

        # Disbale "Radeon™ Boost"
        New-ItemProperty -Path $classKey -Name "KMD_RadeonBoostEnabled" -Value 0 -PropertyType DWord -Force

        # Disable "Radeon™ Chill"
        New-ItemProperty -Path $classKey -Name "KMD_ChillEnabled" -Value 0 -PropertyType DWord -Force

        # Disable "Radeon™ Image Sharpening"
        New-ItemProperty -Path $classKey -Name "KMD_USUEnable" -Value 0 -PropertyType DWord -Force

        # Disable "Radeon™ Enhanced Sync"
        New-ItemProperty -Path $classKey\UMD -Name "TurboSync" -PropertyType Binary -Value ([byte[]](0x30,0x00)) -Force

        # Set "Wait for Vertical Refresh" to "Off, unless application specifies"
        New-ItemProperty -Path $classKey\UMD -Name "VSyncControl" -PropertyType Binary -Value ([byte[]](0x31,0x00)) -Force

        # Disable "Frame rate target control"
        New-ItemProperty -Path $classKey -Name "KMD_FRTEnabled" -Value 0 -PropertyType DWord -Force

        # Set "Anti-Aliasing" to "Use application settings"
        New-ItemProperty -Path $classKey\UMD -Name "EQAA" -PropertyType Binary -Value ([byte[]](0x30,0x00)) -Force

        # Set "Anti-Aliasing Method" to "Multisampling"
        New-ItemProperty -Path $classKey\UMD -Name "ASTT" -PropertyType Binary -Value ([byte[]](0x30,0x00)) -Force

        # Disable "Morphological Anti-Aliasing"
        New-ItemProperty -Path $classKey\UMD -Name "MLF" -PropertyType Binary -Value ([byte[]](0x30,0x00)) -Force

        # Set "Texture Filtering Quality" to "Performance"
        New-ItemProperty -Path $classKey\UMD -Name "TFQ" -PropertyType Binary -Value ([byte[]](0x32,0x00)) -Force

        # Enable "Surface Format Optimization"
        New-ItemProperty -Path $classKey\UMD -Name "SurfaceFormatReplacements" -PropertyType Binary -Value ([byte[]](0x31,0x00)) -Force

        # Set "Tessellation Mode" to "Override application setting"
        New-ItemProperty -Path $classKey\UMD -Name "Tessellation_OPTION" -Value ([byte[]](0x32,0x00)) -PropertyType Binary -Force

        # Set "Maximum Tessallation Level" to "Off"
        New-ItemProperty -Path $classKey\UMD -Name "Tessellation" -Value ([byte[]](0x31,0x00)) -PropertyType Binary -Force

        # Disable "OpenGL Triple Buffering"
        New-ItemProperty -Path $classKey\UMD -Name "EnableTripleBuffering" -Value ([byte[]](0x30,0x00)) -PropertyType Binary -Force

        # Disable "10-Bit Pixel Format"
        New-ItemProperty -Path $classKey\UMD -Name "VisualEnhancements_Capabilities" -Value ([byte[]](0x00,0x00,0x00,0x00)) -PropertyType Binary -Force
        New-ItemProperty -Path $classKey -Name "KMD_10BitMode" -Value 2 -PropertyType DWord -Force

        # Credit: imribiy
        # https://github.com/imribiy/amd-gpu-tweaks
        New-ItemProperty -Path $classKey -Name "StutterMode" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "KMD_EnableAmdFendrOptions" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "KMD_FramePacingSupport" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DalDisableStutter" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableBlockWrite" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableFBCSupport" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableFBCForFullScreenApp" -Value 1 -PropertyType DWord -Force

        New-ItemProperty -Path $classKey -Name "PP_Force3DPerformanceMode" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "PP_ForceHighDPMLevel" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "PP_SclkDeepSleepDisable" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "PP_GfxOffControl" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "PP_ThermalAutoThrottlingEnable" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "PP_EnableRaceToIdle" -Value 0 -PropertyType DWord -Force

        New-ItemProperty -Path $classKey -Name "EnableUlps" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "EnableUlps_NA" -Value 0 -PropertyType String -Force
        New-ItemProperty -Path $classKey -Name "PP_DisableULPS" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "KMD_EnableULPS" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "KMD_ForceD3ColdSupport" -Value 0 -PropertyType DWord -Force

        New-ItemProperty -Path $classKey -Name "EnableAspmL0s" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "EnableAspmL1" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "EnableAspmL1SS" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableAspmL0s" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableAspmL1" -Value 1 -PropertyType DWord -Force

        New-ItemProperty -Path $classKey -Name "DisableGfxClockGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableVceClockGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableSamuClockGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableRomMGCGClockGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableGfxCoarseGrainClockGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableGfxMediumGrainClockGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableGfxFineGrainClockGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableHdpMGClockGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "EnableVceSwClockGating" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "EnableUvdClockGating" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "EnableGfxClockGatingThruSmu" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "EnableSysClockGatingThruSmu" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableXdmaSclkGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DalFineGrainClockGating" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableRomMediumGrainClockGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableNbioMediumGrainClockGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableMcMediumGrainClockGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "IRQMgrDisableIHClockGating" -Value 1 -PropertyType DWord -Force

        New-ItemProperty -Path $classKey -Name "DisableGfxMGLS" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableHdpClockPowerGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableUVDPowerGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableVCEPowerGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableAcpPowerGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableDrmdmaPowerGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableGfxCGPowerGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableStaticGfxMGPowerGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableDynamicGfxMGPowerGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableCpPowerGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableGDSPowerGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableXdmaPowerGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableGFXPipelinePowerGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisableQuickGfxMGPowerGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DisablePowerGating" -Value 1 -PropertyType DWord -Force

        New-ItemProperty -Path $classKey -Name "SMU_DisableMmhubPowerGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "SMU_DisableAthubPowerGating" -Value 1 -PropertyType DWord -Force

        New-ItemProperty -Path $classKey -Name "DalForceMaxDisplayClock" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DalDisableClockGating" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DalDisableDeepSleep" -Value 1 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "DalDisableDiv2" -Value 1 -PropertyType DWord -Force

        New-ItemProperty -Path $classKey -Name "EnableSpreadSpectrum" -Value 0 -PropertyType DWord -Force
        New-ItemProperty -Path $classKey -Name "EnableVcePllSpreadSpectrum" -Value 0 -PropertyType DWord -Force
    }
}