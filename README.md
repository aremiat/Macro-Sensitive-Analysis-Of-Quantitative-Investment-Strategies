Macro-Sensitive Analysis of Quantitative Investment Strategies

This project explores the macroeconomic dynamics of quantitative investment strategies. Its primary objective is to assess how periods of macroeconomic stress influence strategy performance.
Data

The project utilizes data sourced from public economic and financial databases (NBER and Yahoo Finance). Please ensure you have the necessary permissions before using this data.
Methodology
Data Collection

    Universe Construction:
    A universe of 341 US stocks is built with a sector composition closely aligned with that of the S&P 500.

Data Cleaning and Transformation

    Preparation:
    Datasets are cleaned, prepared, and segmented to facilitate detailed analysis.

Modeling

    Quantitative Strategies:
    Two main strategies—Momentum and Value—are applied and their performance is evaluated.

Results Visualization

    Graphical Analysis:
    Charts are generated to interpret trends and identify anomalies in the data.

Strategies
Momentum Strategy

    Computation:
    Calculate the 12-month return (excluding the last month) for each asset.
    Selection:
    Identify the 10 best-performing assets for long positions and the 10 worst-performing assets for short positions.
    Weighting:
    Assign positive weights to the long positions and negative weights to the short positions, normalizing such that the sum of the absolute weights equals 1.

Value Strategy

    Valuation Coefficient:
    Compute the coefficient as follows:
    coeff=PtPt−Pt−5 years
    coeff=Pt​−Pt−5years​Pt​​
    Selection:
    Choose the 10 assets with the lowest coefficient for buying and the 10 assets with the highest coefficient for short selling.
    Weighting:
    Assign and normalize weights similarly to the Momentum Strategy.

Global Architecture

The project is structured into four main modules, following a linear processing flow:

    Data Module:
    Handles loading, filtering, and segmenting historical data.
    Strategies Module:
    Implements the StrategyBase, MomentumStrategy, and ValueStrategy.
    Backtest Module:
    Simulates the portfolio with monthly rebalancing.
    Reporting Module:
    Calculates key performance indicators (such as Sharpe ratio, volatility, and returns) and generates graphical and CSV reports.

Results

    Momentum Strategy:
    Performs better during expansion/recession periods with an average Sharpe ratio of 0.76 compared to 0.04 for the Value Strategy.

    Value Strategy:
    Tends to perform better during inflationary periods, showing an average Sharpe ratio of 0.11 versus 0.045 for the Momentum Strategy.

    Correlation:
    The two strategies exhibit negative correlation:
        −0.23−0.23 during recession/expansion periods.
        −0.35−0.35 during inflationary periods.

    Notably, during periods of inflation, both strategies tend to become even less correlated.
