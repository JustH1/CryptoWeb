//шифруем или дешифруем (true - encr, false-decr)
let encryptOrDecryp = true;
//форма
let form = document.querySelector("form");
//input выбора файла(ов)
let inputField = form.querySelector("#input-field");
//ul лист с файлами
let fileList = document.querySelector("#file-list");
//текстовое поле для вывода информации о количестве загруженных файлов
let countFiles = document.querySelector("#count-files");
//header Encryption
let EncryptionLink = document.querySelector("#EncryptionLink");
//header Decryption
let DecryptionLink = document.querySelector("#DecryptionLink");
//отправка файлов для работы на сервере
let sendFiles = document.querySelector("#send");
//Input for entering the password
let inputPasswd = document.querySelector("#input-passwd");

let fileArray = [];

//обработка события нажатия на кнопку отправки файлов
sendFiles.addEventListener("click", async () => {
    let formData = new FormData();
    fileArray.forEach((element) => {
        formData.append(element.name, element);
    });

    let passwd = (checkEmptyInputPass() === true) ? inputPasswd.trim() : alert("Password is`t to be empty");

    let response = await fetch(`/upload?type=${encryptOrDecryp}&pass=${passwd}`, {
        method: "POST",
        body: formData,
    });

    if (response.ok) {
        alert("Perfect");
    }
    else {
        alert(`${response.status}:${await response.text()}`);
    }
});


//Проверка на пустой пароль
function checkEmptyInputPass() {
    if (inputPasswd.trim() === '') {
        return false;
    }
    return true;
}
//хотим шифровать
document.querySelector("#EncryptionLink").addEventListener("click", () => {
    if (!EncryptionLink.classList.contains("done")) {
        encryptOrDecryp = true;
        EncryptionLink.classList.toggle("done");
        DecryptionLink.classList.remove("done");
    }
});
//хотим дешифровать
document.querySelector("#DecryptionLink").addEventListener("click", () => {
    if (!DecryptionLink.classList.contains("done")) {
        encryptOrDecryp = false;
        EncryptionLink.classList.remove("done");
        DecryptionLink.classList.toggle("done");
    }
});

//вывод информации о ключе
document.querySelector("#passwd-info").addEventListener("click", () => {
    alert(
        "Ключ(пароль) является одним из наиболее важных элементов." +
        "Ключ key представляет собой секретное значение, которое используется для зашифрования и расшифрования данных" +
        "Ключ должен быть использован только для целей шифрования и расшифрования данных, для которых он был предназначен. ПОТЕРЯ КЛЮЧА ПРИВЕДЁТ К НЕВОЗМОЖНОСТИ ДЕШИФРОВКИ ФАЙЛА."
    );
});

form.addEventListener("click", () => {
    inputField.click();
});
//событие выбора файла в форме, добавляет файл в список
inputField.onchange = ({ target }) => {
    let files = target.files;
    for (let index = 0; index < files.length; index++) {
        if (files[index]) {
            addLi(files[index].name);
            fileArray.push(files[index]);
        }
    }
};

//функция добавления элемента списка li с именем файла
function addLi(fileName) {
    let LiElement = document.createElement("li");

    LiElement.innerHTML = `<i class="fa-solid fa-file"></i>
    <p>${fileName}</p><button><i class="fa-solid fa-xmark"></i></button>`;

    LiElement.querySelector("button").addEventListener("click", () => {
        if (Array.from(fileList.children).length == 1) {
            addEmpty();
        }
        let index = Array.from(fileList.children).indexOf(LiElement);
        fileArray.splice(index, 1);
        LiElement.remove();
    });

    countFiles.textContent = "Added files";
    fileList.appendChild(LiElement);
}

//функция изменения текста в countFile если список пуст
function addEmpty() {
    countFiles.textContent = "You haven't added any files yet.";
}
