import pandas as pd
import yfinance as yf
import requests
import time
import os

DATA_PATH = os.path.dirname(__file__) + "/data"

headers = {
    'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:132.0) Gecko/20100101 Firefox/132.0',
    'Accept': 'application/json'
}

data = requests.get('https://www.sec.gov/files/company_tickers.json', headers=headers)

data = data.json()

data = pd.DataFrame(data).T

close_prices_dict = {}
sector_dict = {}

for _, row in data[:1500].iterrows():
    cik_str = row['cik_str']
    ticker = row['ticker']
    title = row['title']

    try:
        # Fetch the last available closing price
        ticker_data = yf.Ticker(ticker)
        # Récupération des informations sous forme de dictionnaire
        info = ticker_data.get_info()  # ou simplement ticker_data.info
        # Extraction du secteur (la clé peut varier selon les données disponibles)
        sector = info.get('sector', None)

        # Récupération des données historiques, en désactivant l'ajustement automatique si nécessaire
        historical_data = ticker_data.history(start="1985-01-02", end="2024-12-31", auto_adjust=False)['Close']

        sector_dict[ticker] = sector
        close_prices_dict[ticker] = historical_data

    except Exception as e:
        print(e)
        continue

    time.sleep(1)

close_prices_df = pd.DataFrame(close_prices_dict)

# Ensure the DataFrame has the correct DateTimeIndex
close_prices_df.index = pd.to_datetime(close_prices_df.index)

# Optionally, if you want to reindex to a common date range:
common_dates = pd.date_range(start=close_prices_df.index.min(), end=close_prices_df.index.max(), freq='B')  # 'B' for business days

# Reindex the DataFrame to align all time series by the common date range
close_prices_df = close_prices_df.reindex(common_dates)

# Fill missing values (optional)
close_prices_df.fillna(method='ffill', inplace=True)  # Forward fill, or use 'bfill' for backward fil

for column in close_prices_df.columns:
    # Check if the first value in the column is NaN
    if pd.isna(close_prices_df[column].iloc[0]):
        # Drop the column if the first value is NaN
        close_prices_df.drop(columns=[column], inplace=True)

close_prices_df.to_csv(f"{DATA_PATH}/univers.csv", index=True)

# Filtrer les tickers présents à la fois dans sector_dict et close_prices_df
common_tickers = [ticker for ticker in close_prices_df.columns if ticker in sector_dict]

# Now, create a DataFrame for the corresponding values from sector_dict for common_tickers
sector_data = {ticker: sector_dict[ticker] for ticker in common_tickers}

# Create the DataFrame from the extracted sector data
sector_df = pd.DataFrame.from_dict(sector_data, orient='index', columns=['Sector'])
sector_counts = sector_df.value_counts()

sector_tickers = sector_df.groupby('Sector').apply(lambda x: x.index.tolist()).to_dict()

output_data = []

total_actions = sum(sector_counts)

for sector, count in sector_counts.items():
    percentage = (count / total_actions)   # Calcul du pourcentage sectoriel
    output_data.append([sector, f"{percentage:.2f}"])

# Créer un DataFrame pour enregistrer les résultats
output_df = pd.DataFrame(output_data, columns=['Secteur', 'Pourcentage'])

# Sauvegarder le DataFrame dans un fichier CSV
output_df.to_csv(f"{DATA_PATH}/secteurs.csv", index=False)