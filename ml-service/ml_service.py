import pandas as pd
import pyodbc
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from sklearn.ensemble import RandomForestClassifier
import joblib
from fastapi import FastAPI
from pydantic import BaseModel

# --- Conexión y carga de datos ---
def load_data():
    conn = pyodbc.connect(
        "Driver={ODBC Driver 18 for SQL Server};"
        "Server=credit-sql,1433;"   # usar nombre del contenedor
        "Database=CreditDB;"
        "UID=sa;"
        "PWD=Your_password123!;"
        "TrustServerCertificate=yes;"
    )
    df = pd.read_sql("SELECT Income, EmploymentMonths, Amount, TermMonths, Status FROM CreditRequests", conn)
    df["Status"] = df["Status"].map({"APROBADO": 1, "RECHAZADO": 0})
    return df

def train_and_save():
    df = load_data()
    X = df[["Income", "EmploymentMonths", "Amount", "TermMonths"]]
    y = df["Status"]

    scaler = StandardScaler()
    X_scaled = scaler.fit_transform(X)

    clf = RandomForestClassifier(n_estimators=200, random_state=42)
    clf.fit(X_scaled, y)

    joblib.dump((scaler, clf), "credit_model.pkl")
    print("✅ Modelo entrenado y guardado como credit_model.pkl")

# Entrenar modelo al iniciar
train_and_save()

# --- FastAPI ---
app = FastAPI(title="Credit ML Predictor")

class CreditRequest(BaseModel):
    income: float
    employment_months: int
    amount: float
    term_months: int

@app.post("/predict")
def predict(req: CreditRequest):
    scaler, clf = joblib.load("credit_model.pkl")
    data = [[req.income, req.employment_months, req.amount, req.term_months]]
    data_scaled = scaler.transform(data)
    pred = clf.predict(data_scaled)[0]
    proba = clf.predict_proba(data_scaled)[0].tolist()
    return {
        "prediction": "APROBADO" if pred == 1 else "RECHAZADO",
        "probabilities": {"RECHAZADO": proba[0], "APROBADO": proba[1]}
    }
