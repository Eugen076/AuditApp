function updateDynamicRanges() {
    const loanType = document.querySelector('[name="LoanType"]').value;
    const isIpotecar = loanType === "Imobiliar-Ipotecar";

    const minAmount = isIpotecar ? 7000 : 5000;
    const maxAmount = isIpotecar ? 1200000 : 250000;
    const minYears = isIpotecar ? 6 : 1;
    const maxYears = isIpotecar ? 30 : 5;

    const amountInput = document.getElementById('amountInput');
    const termInput = document.getElementById('termInput');
    const amountRange = document.getElementById('amountRange');
    const termRange = document.getElementById('termRange');

    // Actualizează valorile min/max
    amountInput.min = minAmount;
    amountInput.max = maxAmount;
    termInput.min = minYears;
    termInput.max = maxYears;

    // Actualizează mesajele text
    amountRange.textContent = `Suma trebuie să fie între ${minAmount.toLocaleString('ro-RO')} și ${maxAmount.toLocaleString('ro-RO')} LEI.`;
    termRange.textContent = `Durata trebuie să fie între ${minYears} și ${maxYears} ani.`;
}
// Căutare client live
const input = document.getElementById("clientSearch");
const list = document.getElementById("clientResults");
const hiddenId = document.getElementById("CustomerId");

input.addEventListener("input", async function () {
    const searchTerm = input.value.trim();
    if (searchTerm.length < 2) {
        list.innerHTML = "";
        list.classList.add("hidden");
        return;
    }

    const response = await fetch(`/Loan/SearchClients?term=${encodeURIComponent(searchTerm)}`);
    const clients = await response.json();

    list.innerHTML = "";
    clients.forEach(client => {
        const li = document.createElement("li");
        li.textContent = client.name;
        li.dataset.id = client.id;
        li.className = "px-3 py-2 hover:bg-gray-100 cursor-pointer";
        list.appendChild(li);
    });

    list.classList.remove("hidden");
});

list.addEventListener("click", function (e) {
    if (e.target.tagName === "LI") {
        input.value = e.target.textContent;
        hiddenId.value = e.target.dataset.id;
        list.classList.add("hidden");
    }
});

document.addEventListener("click", function (e) {
    if (!list.contains(e.target) && e.target !== input) {
        list.classList.add("hidden");
    }
});

// Rulează o dată la încărcare și apoi la schimbare tip credit
document.addEventListener("DOMContentLoaded", updateDynamicRanges);
document.querySelector('[name="LoanType"]').addEventListener('change', updateDynamicRanges);