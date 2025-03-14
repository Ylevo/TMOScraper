# TMO Scraper

Simple WinForms application to scrape the spanish website TMO.

It uses [html-agility-pack](https://github.com/zzzprojects/html-agility-pack) in the main implementation to fetch & parse pages and [puppeteer-sharp](https://github.com/hardkoded/puppeteer-sharp) as a fallback which requires Chromium. It will be downloaded once (path : AppData/Local/TMOScraper) on the first start and checked if present on subsequent runs.

## Usage

Download the [latest version](https://github.com/Ylevo/TMOScraper/releases/latest) from the releases page and run the exe. 

- You may scrape more than one URL at a time by entering multiple of them, one URL per line.
- If a scraped chapter's folder already exists, it will be skipped.
- Hover your mouse over the UI elements to display the help, such as the mango URLs textbox to show the supported URLs.

## Options

You can access them by clicking on the options button on the topbar.

- `TMO Domain` It is as it says, the current domain of TMO. Change it if they do as it happens sometimes.
- `Use Puppeteer` Behaviour to use when it comes to puppeteer. If it keeps falling back to puppeteer (because the default implementation is broken/doesn't work anymore), change it to default.
- `Convert images` You may change it depending on what title you scrape. PNG 4 bpp is best for B&W but will turn any colored page in B&W as well. You may also disable it if you love webp images.
- `Split images` Autosplitting images higher in height than 10k pixels.

The rest of the options are mostly self-explanatory. The different delays available are to avoid hitting TMO's ratelimits. Default values will ensure you never get banned but you may go wilder.

## Credits

- [Html Agility Pack](https://github.com/zzzprojects/html-agility-pack)
- [Puppeteer Sharp](https://github.com/hardkoded/puppeteer-sharp) - to give the console a bit of colour.
- [Polly](https://github.com/App-vNext/Polly)
- [Serilog](https://github.com/serilog/serilog)
