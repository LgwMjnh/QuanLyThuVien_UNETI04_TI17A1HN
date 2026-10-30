
function createPasswordInput(inputId, placeholder = "Nhập mật khẩu") {
    const container = document.getElementById(inputId + "Container");

    if (!container) return;

    container.innerHTML = `
        <div class="input-group">
            <input type="password"
                   id="${inputId}"
                   name="${inputId}"
                   class="form-control"
                   placeholder="${placeholder}" />

            <button type="button"
                    class="btn btn-outline-secondary"
                    onclick="togglePassword('${inputId}', this)"
                    aria-label="Hiện hoặc ẩn mật khẩu">
                Hiện
            </button>
        </div>
    `;
}

function togglePassword(inputId, button) {
    const input = document.getElementById(inputId);

    if (!input) return;

    if (input.type === "password") {
        input.type = "text";
        button.textContent = "Ẩn";
    } else {
        input.type = "password";
        button.textContent = "Hiện";
    }
}
