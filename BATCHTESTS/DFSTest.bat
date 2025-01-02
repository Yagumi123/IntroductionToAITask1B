@echo off
echo Running batch test for DFS...

:: Path to your executable and test file
set EXE_PATH=IntroToAIAssignment1.exe
set TEST_FILE=NAVFILES\RobotNavTEST.txt

echo Method: dfs
"%EXE_PATH%" "%TEST_FILE%" dfs

echo DFS test completed.
pause
