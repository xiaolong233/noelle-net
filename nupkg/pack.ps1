$currentPath = (Get-Item -Path "./" -Verbose).FullName
$rootPath = Join-Path $currentPath "../"

# 设置输出目录
$outputDir = "nupkgs"

# 创建输出目录（如果不存在）
if (-Not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir
}
Remove-Item (Join-Path $currentPath $outputDir *.nupkg)

# 查找所有项目文件
$frameworkPath = Join-Path $rootPath "framework"
$projectFiles = Get-ChildItem -Path $frameworkPath -Recurse -Filter *.csproj

foreach ($projectFile in $projectFiles) {
    # 打印正在处理的项目
    Write-Output "正在打包项目: $($projectFile.FullName)"
    
    # 使用 dotnet pack 命令打包项目
    dotnet pack $projectFile.FullName `
        --configuration Release `
        -o $outputDir
}

Write-Output "打包完成。生成的 NuGet 包保存在目录: $outputDir"