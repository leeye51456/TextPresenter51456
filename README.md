# TextPresenter51456 (Beta)

간단한 텍스트를 송출하는 도구입니다. 아래 내용은 0.2.13.27(베타) 버전 기준으로 작성했습니다. 베타 버전의 기능은 하위 호환성을 보장하지 않습니다. 베타 버전에서 추가되는 기능은 언제든 변경되거나 삭제될 수 있습니다.

## 0.2.13.27(베타) 중요 변경 사항

- 설정 파일 이름이 `TextPresenter51456.settings`에서 `TextPresenter51456.conf`로 변경되고, 설정 포맷도 조금 변경되었습니다.
- 이전 버전에서 업데이트 시 설정 파일 이름을 `TextPresenter51456.conf`로 변경하고 프로그램을 실행하면 새 포맷으로 자동 변경됩니다.

## 필요 환경

### 권장 환경(테스트 환경)

- Windows 10
  - 정식 출시된 지 1개월 이상 지난, 최신 버전
  - 업데이트가 사용자에 의해 미뤄지지 않고 꾸준히 이루어진 환경
- 2개 이상의 화면 출력 장치

### 최소 환경

- Windows 7 이상
- Microsoft .NET Framework 4.5.2 이상 버전 설치됨(Windows 10에 기본 탑재)

## 사용법

### 텍스트 파일 열기

- "파일(F) - 열기(O)..." 메뉴를 선택하여 텍스트 파일을 불러옵니다. 단축키는 <kbd>Ctrl</kbd>+<kbd>O</kbd>입니다.
- 텍스트 파일을 메인 창으로 끌어서 간편하게 열 수도 있습니다.
- 아무 파일도 열지 않은 상태에서는 페이지 리스트 영역을 더블클릭하여 파일을 열 수도 있습니다.
- 텍스트 파일 작성 방법은 아래와 데모 파일을 참고하세요.

### 송출 창 열기·닫기

오른쪽 위의 "**송출 창(Shift+ESC)** 버튼이나 <kbd>Shift</kbd>+<kbd>ESC</kbd>를 누르면 송출 창이 화면에 꽉 차게 열립니다. 화면이 하나인 경우 안내 메시지가 나타난 후 열립니다. 기본적으로 송출 창이 열리는 화면은 마지막 화면이며, 설정 메뉴를 통해 변경할 수 있습니다.

### 키보드를 이용한 송출

- 아무 것도 입력하지 않은 상태에서 <kbd>Enter</kbd> 키를 누르면 PVW에 올라와 있던 페이지가 PGM으로 넘어오면서 송출 창으로 송출됩니다.
- 내보낸 텍스트를 지우려면 <kbd>.</kbd> 키를 누릅니다.
- 숫자 키를 눌러 숫자를 입력하고 <kbd>Enter</kbd> 키를 누르면 해당 페이지가 PVW로 올라옵니다.
  - <kbd>0</kbd>을 입력한 상태에서 <kbd>Enter</kbd> 키를 누르면 현재 PVW에 올라와 있는 페이지의 앞 페이지가 PVW로 올라옵니다.
  - 존재하지 않는 페이지를 입력한 상태에서 <kbd>Enter</kbd> 키를 누르면 아무 일도 일어나지 않고, 입력한 내용만 사라집니다.
- <kbd>←</kbd>/<kbd>→</kbd> 키를 누르면 현재 PVW에 대기 중인 페이지의 이전/다음 페이지가 PVW로 올라옵니다.
- <kbd>↑</kbd>/<kbd>↓</kbd> 키를 누르면 화면상 현재 PVW의 위·아래쪽 페이지가 PVW로 올라옵니다.
- <kbd>Shift</kbd>+<kbd>↑</kbd>/<kbd>↓</kbd> 키를 누르면 페이지 리스트 영역이 한 줄씩 스크롤됩니다.

### 마우스를 이용한 송출

- 페이지 리스트에서 아무 페이지나 클릭하면 그 페이지가 PVW로 올라옵니다.
- "**Cut**" 버튼을 누르면 PVW에 올라와 있던 페이지가 PGM으로 넘어오면서 송출 창으로 송출됩니다.
- 내보낸 텍스트를 지우려면 "**Clear**" 버튼을 누릅니다.
- 페이지 리스트에서 특정 페이지를 바로 내보내려면 그 페이지를 더블클릭합니다.

### 텍스트 자유 송출

- 자유 송출 텍스트 상자에 텍스트를 입력하고, 그 위의 "**송출(Ctrl+Enter)**" 버튼을 누르거나 <kbd>Ctrl</kbd>+<kbd>Enter</kbd>를 누르면 송출 창에 그 내용이 송출됩니다. 페이지 번호 자리에는 페이지 번호 대신 "자유송출"이 표시됩니다.
- "마지막 x줄 송출" 부분의 숫자를 변경하고 "적용" 버튼을 누르면 텍스트 자유 송출 기능 사용 시 출력되는 최대 줄 수를 지정할 수 있습니다.
- "**즉시 업데이트**"에 체크하면 텍스트가 변경되는 대로 즉시 반영됩니다.
- 자유 송출 상태에서 "**Clear**" 버튼을 누르면 송출된 내용이 지워지며, 즉시 업데이트가 켜져 있는 경우 꺼집니다.
- 자유 송출 텍스트 상자에 포커스가 있을 때 일부 단축키가 변경됩니다.
  - 작동하지 않는 단축키: <kbd>0</kbd>-<kbd>9</kbd>, <kbd>Enter</kbd>, <kbd>←</kbd>/<kbd>↓</kbd>/<kbd>↑</kbd>/<kbd>→</kbd>
  - 변경되는 단축키: <kbd>.</kbd> → <kbd>Ctrl</kbd>+<kbd>.</kbd>
- 자유 송출 텍스트 상자에서 포커스를 빼려면 <kbd>ESC</kbd> 키를 누르거나, 텍스트 상자 바깥 영역을 클릭합니다.

### 텍스트 파일 작성 방법

파일 확장자는 상관이 없으나 `*.txt`가 아니면 열 때 조금 번거로울 수 있습니다. Windows 환경에서는 메모장(notepad)로 작업하면 됩니다. 텍스트 인코딩은 기본적으로 운영체제 기본 인코딩을 따릅니다.

#### 기본 규칙

- 행 사이를 한 줄 이상 비우면 페이지가 분리됩니다.

```
행 사이를 한 줄 이상 비우면

페이지가 분리됩니다.
```

- 페이지가 분리되지 않는다면, 빈 것처럼 보이는 줄에 공백 문자가 있는지 확인하십시오. 한 줄 안에 공백 문자 등 어떤 문자라도 있으면 비어 있지 않은 줄로 간주합니다.

- 파일 맨 앞과 맨 뒤의 모든 줄 바꿈은 무시합니다. 대신 마지막에 빈 페이지 하나가 항상 추가됩니다.

- 중간에 빈 페이지를 만들려면 빈 줄 사이에 ` `(Space) 같은 공백 문자나 `#`을 넣습니다.

```
(이전 페이지)

(# 또는 공백 문자)

(다음 페이지)
```

#### 제목 페이지

- 제목 페이지를 만들려면 앞에 `#`을 넣습니다. 
- 제목 페이지는 다른 색으로 표시됩니다. 기본값은 노란색이며, 설정에서 변경할 수 있습니다.

```
# 제목 페이지

일반 페이지
```

#### 주석(Comment)

- 주석을 넣어 내용을 가리려면 가리려는 내용 앞에 `//`를 붙입니다. `//` 뒤에 작성된 내용은 표시되지 않습니다.
- 행의 맨 앞에 `//`를 붙이면 그 행은 표시되지 않습니다.
- 한 페이지의 모든 행의 맨 앞에 `//`를 붙이면 그 페이지는 표시되지 않습니다.

#### 문법 적용 피하기

- 기본 규칙을 제외한 문법을 피하려면, 해당 구문 앞에 `\\`(폰트에 따라 백슬래시 또는 원화 기호 등으로 보일 수 있음)를 붙입니다.
  - 예를 들어, 페이지 맨 앞에 `#`을 넣으면 `#`이 들어가지 않고 해당 페이지가 제목 페이지가 되는데, 이때 `#`을 그대로 넣고 싶다면 `#` 대신 `\\#`을 넣어야 합니다.

```
\# 제목 페이지가 아니면서 앞에 #이 붙는 페이지

# # 제목 페이지이면서 앞에 #이 붙는 페이지
```
