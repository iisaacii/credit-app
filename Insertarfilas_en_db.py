import pyodbc
import random



# Conexión a SQL Server
conn = pyodbc.connect(
    "Driver={ODBC Driver 18 for SQL Server};"
    "Server=localhost,1433;"
    "Database=CreditDB;"
    "UID=sa;"
    "PWD=Your_password123!;"
    "TrustServerCertificate=yes;"
)
cursor = conn.cursor()

def generar_datos(n=5000):
    for _ in range(n):
        income = random.randint(2000, 80000)       # ingreso mensual
        empl = random.randint(0, 120)              # meses de antigüedad
        amount = random.randint(1000, 200000)      # monto del crédito
        term = random.choice([6, 12, 24, 36])      # plazo en meses

        # scoring simple
        score = 0
        score += 50 if income >= 30000 else 30 if income >= 15000 else 10
        score += 30 if empl >= 24 else 15 if empl >= 12 else 5 if empl > 0 else 0
        if amount <= income * 6:
            score += 20

        status = "APROBADO" if score >= 60 else "RECHAZADO"

        cursor.execute("""
            INSERT INTO CreditRequests (ClientId, Amount, TermMonths, Income, EmploymentMonths, Status, Score)
            VALUES (1, ?, ?, ?, ?, ?, ?)
        """, (amount, term, income, empl, status, score))
    conn.commit()
    print(f"{n} registros insertados")

generar_datos(5000)
