// Ensure jQuery is loaded. If not, include it, e.g.:
// <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>

// Function to adjust header padding for scrollbar width
// This keeps the header columns aligned with the body columns when a scrollbar appears.
$(window).on("load resize ", function () {
    var scrollWidth = $('.tbl-content').width() - $('.tbl-content table').width();
    $('.tbl-header').css({ 'padding-right': scrollWidth });
}).resize(); // .resize() runs it once on load

// Function to fetch and update portfolio total values dynamically
async function updatePortfolioTotalValues() {
    try {
        const response = await fetch('/Portfolio/GetLivePortfolioTotalValues');
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json(); // Expected to be an array of { portfolioId, totalValue }

        data.forEach(portfolioUpdate => {
            const totalValueElement = document.getElementById(`totalValue_${portfolioUpdate.portfolioId}`);
            if (totalValueElement) {
                // Format as currency (Indian Rupees in this case, adjust locale if needed)
                totalValueElement.textContent = portfolioUpdate.totalValue.toLocaleString('en-IN', { style: 'currency', currency: 'INR' });
            }
        });

    } catch (error) {
        console.error("Error fetching live portfolio total values:", error);
        // You could add a user-facing error message here if the update is critical
    }
}

// Call the function immediately when the page loads
updatePortfolioTotalValues();

// Set an interval to update the values every 5 seconds (5000 milliseconds)
setInterval(updatePortfolioTotalValues, 5000);