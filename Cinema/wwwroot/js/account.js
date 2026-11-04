
function togglePassword(id) {
    const password = document.getElementById(id);

    if (password.type === "password") {
        password.type = "text";
    } else {
        password.type = "password";
    }
}

function showMessage(message, type) {
    const messageBox = document.getElementById("formMessage");

    if (!messageBox) return;

    messageBox.textContent = message;
    messageBox.className = "form-message " + type;
}

function validateForm(event) {
    event.preventDefault();

    const email = document.getElementById("email");
    const password = document.getElementById("password");
    const confirmPassword = document.getElementById("confirmPassword");

    if (email && !email.value.trim()) {
        showMessage("Please enter your email address.", "error");
        return false;
    }

    if (password && password.value.length < 6) {
        showMessage("Password must be at least 6 characters.", "error");
        return false;
    }

    if (password && confirmPassword &&
        password.value !== confirmPassword.value) {
        showMessage("Passwords do not match.", "error");
        return false;
    }

    showMessage("Form validation completed successfully!", "success");

    return false;
}
