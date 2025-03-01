# Macro-Sensitive-Analysis-Of-Quantitative-Investement-Strategies

This project explores the macroeconomic analysis of quantitative investment strategies. The objective is to assess how macroeconomic factors influence the performance of quantitative strategies and to optimize their robustness against economic cycles.

📂 Data

The data used in this project comes from public economic and financial source (NBER and Yahoo Finance). Ensure you have the necessary permissions before using them.

📈 Methodology

Data Collection: Create a universe of 341 US stocks, with a sector composition that is close to the SP500.

Data Cleaning and Transformation: Preparing datasets for analysis and segmenting data.

Modeling: Applying quantitative strategies and evaluating performance.

Results Visualization: Creating charts to interpret trends and anomalies.

📊 Value and Momentum Strategies

Momentum Strategy:

Compute the 12-month return (excluding the last month) for each asset.

Select the 10 best-performing assets for buying and the 10 worst-performing assets for short selling.

Assign positive/negative weights and normalize (sum of absolute values = 1).

Value Strategy:

Compute a valuation coefficient:

coeff = Pt / (Pt - Pt-5 years)

Select the 10 assets with the lowest coefficient for buying and the 10 with the highest coefficient for short selling.

Assign weights and normalize.

🔧 Global Architecture

The project is structured around four main modules, following a linear processing flow:

Module Data → Module Strategies → Module Backtest → Module Reporting

Module Data: Loading, filtering, and segmenting historical data.

Module Strategies: Implementation of StrategyBase, MomentumStrategy, and ValueStrategy.

Module Backtest: Portfolio simulation with monthly rebalancing.

Module Reporting: Calculation of key performance indicators (Sharpe ratio, volatility, returns) and generation of graphical/CSV reports.

Results

Overall, our results indicates that Momentum performs better during expansion/recesson than value with an average Sharpe ratio of 0.76 compare to 0.04 for Value.
On the other hand, during period of inflation, Value tends to perform better with an average Sharpe ratio of 0.11 compare to 0.045 for Momentum.
Both strategies are negatively correlated (-0.23 in recession/expansion and -0.35 in inflation up-down). 
It is interresting to note that during periods of inflation both strategies tend to uncorrolate even more.
