@echo off
echo Running batch test for A*...

:: Path to your executable and test file
set EXE_PATH=IntroToAIAssignment1.exe
set TEST_FILE=NAVFILES\RobotNavTEST.txt

echo Method: astar
"%EXE_PATH%" "%TEST_FILE%" astar

echo A* test completed.
pause
