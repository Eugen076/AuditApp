const modal = document.getElementById("authModal");
const closeBtn = document.getElementById("closeModal");
const authForm = document.getElementById("authForm");
const clientIdInput = document.getElementById("clientId");
const authError = document.getElementById("authError");

const detailsModal = document.getElementById("detailsPopup");
const detailsContent = document.getElementById("detailsContent");
const closeDetails = document.getElementById("closeDetails");

document.querySelectorAll(".open-auth-modal").forEach(btn => {
    btn.addEventListener("click", () => {
        const id = btn.getAttribute("data-id");
        clientIdInput.value = id;
        modal.classList.remove("hidden");
        authError.classList.add("hidden");
        authForm.password.value = "";
    });
});

closeBtn.addEventListener("click", () => {
    modal.classList.add("hidden");
});

closeDetails.addEventListener("click", () => {
    detailsModal.classList.add("hidden");
});

authForm.addEventListener("submit", async function (e) {
    e.preventDefault();

    const id = clientIdInput.value;
    const password = authForm.password.value;
    authError.classList.add("hidden");

    try {
        const response = await fetch('/Customers/Reauth', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ id, password })
        });

        if (response.ok) {
            const data = await response.json();
            console.log("DATA RETURNED:", data);

            modal.classList.add("hidden");
            
            detailsContent.innerHTML = `
                <div><strong>Nume complet:</strong> ${data.fullname}</div>
                <div><strong>Telefon:</strong> ${data.phone}</div>    
                <div><strong>CNP:</strong> ${data.cnp}</div>
                <div><strong>Data nașterii:</strong> ${data.dob}</div>
                <div><strong>Adresă:</strong> ${data.address}</div>
                <div><strong>Creat la:</strong> ${data.created}</div>
            `;
            detailsModal.classList.remove("hidden");

            setTimeout(() => {
                detailsModal.classList.add("hidden");
            }, 45000);
        } else {
            const errorData = await response.json();
            console.warn("Răspuns eroare:", errorData);
            authError.classList.remove("hidden");
        }
    } catch (err) {
        console.error("Eroare la autentificare:", err);
        authError.classList.remove("hidden");
    }
});
