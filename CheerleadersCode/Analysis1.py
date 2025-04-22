# condition: 
#   <45     40% left, 60% right
# 45< <90   40% right, 60% left
# 90< <95   all right
# 95< <100  all left
# -----------------------------------------------------------------------------
# Pressed key:
# 0 no press
# 1 right
# 2 left
# 3 down
# -----------------------------------------------------------------------------
# Color
# 1: right hand blue, left hand yellow
# 2: right hand yellow, left hand blue
# =============================================================================
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
# =============================================================================
XLSXdata = pd.read_excel('Cheerleaders.xlsx')
XLSXdata = XLSXdata.drop(['Timestamp', 'Basic Information 1' , 'Basic Information 2' , 'Basic Information 3'], axis=1)
XLSXdata = XLSXdata.iloc[0:8,0:320]
Condition = np.empty((8,320))
PressedKey = np.empty((8,320))
Time = np.empty((8,320))
Color = np.empty((8,320))
for a1 in np.arange(0,8):
    for a2 in np.arange(0,320):
        dataTemp = XLSXdata.iloc[a1,a2]
        dataSplit = dataTemp.split(',')
        Condition[a1,a2] = dataSplit[0]
        PressedKey[a1,a2] = dataSplit[1]
        Time[a1,a2] = dataSplit[2]
        Color[a1,a2] = dataSplit[3]
        
Condition_Train = Condition[:,0:20]
PressedKey_Train = PressedKey[:,0:20]
Time_Train = Time[:,0:20]
Color_Train = Color[:,0:20]

Condition_Test = Condition[:,20:320]
PressedKey_Test = PressedKey[:,20:320]
Time_Test = Time[:,20:320]
Color_Test = Color[:,20:320]

Condition_Test_Catch = Condition_Test.copy()
Condition_Test_Main = Condition_Test.copy()
Condition_Test_Catch[Condition_Test<90]='nan'
Condition_Test_Main[Condition_Test>90]='nan'
PressedKey_Test_Catch = PressedKey_Test.copy()
PressedKey_Test_Main = PressedKey_Test.copy()
PressedKey_Test_Catch[Condition_Test<90]='nan'
PressedKey_Test_Main[Condition_Test>90]='nan'
Time_Test_Catch = Time_Test.copy()
Time_Test_Main = Time_Test.copy()
Time_Test_Catch[Condition_Test<90]='nan'
Time_Test_Main[Condition_Test>90]='nan'
Color_Test_Catch = Color_Test.copy()
Color_Test_Main = Color_Test.copy()
Color_Test_Catch[Condition_Test<90]='nan'
Color_Test_Main[Condition_Test>90]='nan'

CorrectCatch = PressedKey_Test_Catch==0
ttt = np.nansum(CorrectCatch,axis=1)/np.nansum(PressedKey_Test_Catch,axis=1)
