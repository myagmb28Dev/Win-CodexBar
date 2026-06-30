# WindexBar

WindexBar is a small Windows tray app for quickly checking Codex usage and status.

<img width="335" height="505" alt="kr" src="https://github.com/user-attachments/assets/c545974d-044b-4b73-aaeb-5472315f8d1f" />
<img width="341" height="507" alt="eng" src="https://github.com/user-attachments/assets/8cc6a0ca-178b-43ce-8779-8831e01842b9" />

## English

### Install

Download and run `WindexBarSetup.exe` from the GitHub Releases page.

### Requirements

- Codex CLI: WindexBar reads Codex usage through `codex app-server`, so the `codex` command must be available on `PATH`.
- Install Codex CLI: [Codex CLI setup](https://developers.openai.com/codex/cli)
  - Windows PowerShell: `powershell -ExecutionPolicy ByPass -c "irm https://chatgpt.com/codex/install.ps1 | iex"`
  - Node/npm: `npm install -g @openai/codex`
  - Bun: `bun install -g @openai/codex`
  - Homebrew: `brew install --cask codex`
  - Other downloads: [Codex releases](https://github.com/openai/codex/releases/latest)

### Usage

Run the app, then WindexBar appears as an icon in the system tray.

- Left click: open the status window
- Right click: open Settings or Quit
- Alt+O: hide or show the WindexBar window

### Development

- SDK: `.NET SDK 10.0.301` (`global.json`)
- Local SDK install: [dotnet-install scripts](https://learn.microsoft.com/dotnet/core/tools/dotnet-install-script)
- If the local SDK is missing, `run.cmd` and `build-installer.cmd` use the system-installed `dotnet` or the `dotnet` on `PATH`.

Run:

```powershell
.\run.cmd
```

Test:

```powershell
dotnet test .\tests\WindexBar.Core.Tests\WindexBar.Core.Tests.csproj -p:NuGetAudit=false
```

## 한국어

WindexBar는 Codex 사용량과 상태를 Windows 트레이에서 빠르게 확인하는 작은 앱입니다.

### 설치

GitHub Releases 페이지에서 `WindexBarSetup.exe`를 내려받아 실행합니다.

### 요구사항

- Codex CLI: WindexBar는 `codex app-server`로 Codex 사용량을 읽으므로 `codex` 명령이 `PATH`에서 실행 가능해야 합니다.
- Codex CLI 설치: [Codex CLI setup](https://developers.openai.com/codex/cli)
  - Windows PowerShell: `powershell -ExecutionPolicy ByPass -c "irm https://chatgpt.com/codex/install.ps1 | iex"`
  - Node/npm: `npm install -g @openai/codex`
  - Bun: `bun install -g @openai/codex`
  - Homebrew: `brew install --cask codex`
  - 기타 다운로드: [Codex releases](https://github.com/openai/codex/releases/latest)

### 사용

앱을 실행하면 시스템 트레이에 WindexBar 아이콘이 표시됩니다.

- 왼쪽 클릭: 상태 창 열기
- 오른쪽 클릭: Settings 또는 Quit 열기
- Alt+O: WindexBar 창 숨기기 또는 다시 나타내기

### 개발

- SDK: `.NET SDK 10.0.301` (`global.json`)
- 로컬 SDK 설치: [dotnet-install scripts](https://learn.microsoft.com/dotnet/core/tools/dotnet-install-script)
- 로컬 SDK가 없으면 `run.cmd`와 `build-installer.cmd`는 시스템에 설치된 `dotnet` 또는 `PATH`의 `dotnet`을 사용합니다.

실행:

```powershell
.\run.cmd
```

테스트:

```powershell
dotnet test .\tests\WindexBar.Core.Tests\WindexBar.Core.Tests.csproj -p:NuGetAudit=false
```
