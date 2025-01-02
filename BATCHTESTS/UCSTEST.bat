@echo off
echo Running batch test for UCS...

:: Path to your executable and test file
set EXE_PATH=IntroToAIAssignment1.exe
set TEST_FILE=NAVFILES\RobotNavTEST.txt

echo Method: ucs
"%EXE_PATH%" "%TEST_FILE%" ucs

echo UCS test completed.
pause
