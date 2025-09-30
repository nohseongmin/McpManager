사용자는 개인 PC 환경에서 gemini Cli를 사용함. 
그런데 MCP 서버 관리가 쉽지 않음(추가와 삭제가 번거로움)
그래서 gemini cli를 사용자를 위한 mcp 관리앱을 c# 네이티브로 만들거임

기능
1. gemini cli 지원 기능
	1. Node.js 설치
		1. 설치 파일을 프로그램 내부에 놓고 자동 실행되게끔 구성
	2. gemini cli 설치 명령어 Powershell(관리자권한) 에서 실행. 백그라운드로
		1. npm install -g @google/gemini-cli 
		2. Set-ExecutionPolicy RemoteSigned
		3. Y를 선택해 권한 설정 변경
		4. 완료시 완료되었다고 화면에 표시되어야함
	3. gemini cli 실행 버튼 
		1. cmd창을 열어(바탕화면을 기준으로)gemini cli 시작
		2. bat파일로 구성해 실행하는 방식
	4. 기타 매뉴얼
		1. 자주 묻는 질문, 개발자 contact 등 편의 기능
2. MCP 서버 관리 기능
	1. 현재 설치 가능한 MCP 서버의 목록 보여주기
		1. 현재는 Github MCP, Google Drive MCP 만 지원
		2. 검은색 MCP 추가 버튼은 무시하기
	2. MCP 서버 목록 우측의 설치 버튼 눌러서 간단하게 설치
		1. 설치를 누르면 백그라운드cmd에서 명령어를 실행해 설치
		2. npx -y @smithery/cli@latest install @smithery-ai/github --client gemini-cli --key (api키)
		3. 이런식으로 설치 가능, 하지만 api키 발급받는 방법 지원이 필요
		4. github api키 발급 매뉴얼을 api키 입력 칸 위에 두어 바로 누를수있게 링크로 정리
	3. 사용자가 API키를 수정할 수 있도록 지원
		1. 클릭 시 수정 메뉴 나옴.
		2. 문자열 입력 칸과 적용 버튼 두개면 충분함
	4. MCP 삭제 기능
		1. gemini-cli의 setting.json(C:\Users\username\.gemini 에 위치) 에서 삭제
		2. 특정 부분을 삭제해서 연결을 없애는 식으로 동작
3. 다국어 지원 기능
	1. 우측상단 단추를 눌러 한국어/영어 변경가능
	2. 기본적으로 한국어로 되어있음




상상도
![[Pasted image 20250930021857.png]]