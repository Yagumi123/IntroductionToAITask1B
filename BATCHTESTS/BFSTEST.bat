@echo off
echo Running batch test for BFS...

:: Path to your executable and test file
set EXE_PATH=IntroToAIAssignment1.exe
set TEST_FILE=NAVFILES\RobotNavTEST.txt

echo Method: bfs
"%EXE_PATH%" "%TEST_FILE%" bfs

echo BFS test completed.
pause