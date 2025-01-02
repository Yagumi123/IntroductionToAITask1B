@echo off
echo Running batch test for GBFS...

:: Path to your executable and test file
set EXE_PATH=IntroToAIAssignment1.exe
set TEST_FILE=NAVFILES\RobotNavTEST.txt

echo Method: gbfs
"%EXE_PATH%" "%TEST_FILE%" gbfs

echo GBFS test completed.
pause
