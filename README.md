# WindexBar

Codex 사용량 한도를 Windows 트레이에서 빠르게 확인하는 작은 앱입니다.

## 설치

GitHub Release에서 `WindexBarSetup.exe`를 내려받아 실행합니다.

## 사용

앱을 실행하면 시스템 트레이에 아이콘이 표시됩니다.

- 왼쪽 클릭: 상태 창 열기
- 오른쪽 클릭: Settings, Quit
- ALT + O 단축키로 창 숨기기, 나타내기 가능

## 개발

- SDK: `.NET SDK 10.0.301` (`global.json`)
- 로컬 SDK 설치: [dotnet-install scripts](https://learn.microsoft.com/dotnet/core/tools/dotnet-install-script)
- 로컬 SDK가 없으면 `run.cmd`와 `build-installer.cmd`는 시스템 설치 또는 PATH의 `dotnet`을 사용합니다.

실행:

```powershell
.\run.cmd
```

테스트:

```powershell
dotnet test .\tests\WindexBar.Core.Tests\WindexBar.Core.Tests.csproj -p:NuGetAudit=false
```
