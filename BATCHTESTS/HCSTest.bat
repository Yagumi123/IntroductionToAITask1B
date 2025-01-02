@echo off
echo Running batch test for HCS...

:: Path to your executable and test file
set EXE_PATH=IntroToAIAssignment1.exe
set TEST_FILE=TESTFOLDER\RobotNavTEST.txt

echo Method: hcs
"%EXE_PATH%" "%TEST_FILE%" hcs

echo HCS test completed.
pause
