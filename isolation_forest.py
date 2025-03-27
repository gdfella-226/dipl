import pandas as pd
import numpy as np
from sklearn.ensemble import IsolationForest
from sklearn.preprocessing import StandardScaler, OneHotEncoder
from sklearn.pipeline import Pipeline
from joblib import dump
from data_processer import fit_data


file_name = "upd-ETW-test.csv"
fit_data(outfile=file_name)
df = pd.read_csv(file_name, sep=';')
df.fillna(0, inplace=True)
print(df)

num_features = ['Event ID', 'Channel', 'Level', 'Opcode', 'Task', 'PID', 'TID', 'Processor Number', 'Instance ID', 'Parent Instance ID', 'Related Activity ID', 'Clock-Time', 'Kernel(ms)', 'User(ms)']
#cat_features = ['country', 'payment_method']
cat_features = ['Event Name', 'Version', 'Type', 'Keyword', 'Activity ID', 'User Data']

df = pd.get_dummies(df, columns=cat_features, drop_first=True)

scaler = StandardScaler()
df[num_features] = scaler.fit_transform(df[num_features])

model = IsolationForest(n_estimators=200, contamination=0.02, random_state=42)
model.fit(df[num_features])

df['anomaly_score'] = model.decision_function(df[num_features])
df['is_anomaly'] = model.predict(df[num_features])
df['is_anomaly'] = df['is_anomaly'].map({1: 0, -1: 1})

suspicious_orders = df[df['is_anomaly'] == 1]
print(df)

