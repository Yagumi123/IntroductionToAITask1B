@echo off
echo Running batch tests for all algorithms...

:: Path to your executable
set EXE_PATH=IntroToAIAssignment1.exe

:: Path to the test file
set TEST_FILE=NAVFILES\RobotNavTEST.txt

:: Algorithms to test
set METHODS=dfs bfs astar ucs gbfs hcs

:: Loop through each method
for %%M in (%METHODS%) do (
    echo Running test for algorithm: %%M
    "%EXE_PATH%" "%TEST_FILE%" %%M
    echo.
)

echo All batch tests completed.
pause