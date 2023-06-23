$(document).ready(async function () {
  data = await leggiticket();
  aggiornaTabella();
});

let data;

async function aggiornaTabella() {
  j = 0;
  let projectTable = $("#projectTable");
  projectTable.innerHTML = "";
  let tableBody = $("#tableBody");
  tableBody.empty();

  for (i = 0; i < data.length; i++) {
    row = document.createElement("tr");

    idCell = document.createElement("td");
    idCell.innerHTML = data[i].ID;
    row.appendChild(idCell);

    responsabileCell = document.createElement("td");
    responsabileCell.innerHTML = data[i].Responsabile;
    row.appendChild(responsabileCell);

    titoloCell = document.createElement("td");
    titoloCell.innerHTML = data[i].Titolo;
    row.appendChild(titoloCell);

    startDateCell = document.createElement("td");
    startDateCell.innerHTML = data[i].StartDate;
    row.appendChild(startDateCell);

    endDateCell = document.createElement("td");
    endDateCell.innerHTML = data[i].EndDate;
    row.appendChild(endDateCell);

    workEffortCell = document.createElement("td");
    workEffortCell.innerHTML = data[i].WorkEffort;
    row.appendChild(workEffortCell);

    tableBody.append(row);
  }
}

async function leggiticket() {
  let file = 'http://127.0.0.1:5500/progetti.json';
  const response = await fetch(file);
  data = await response.json();

  return data;
}

function ordinaDati(campo) {
  data = data.sort(function (a, b) {
    if (a[campo] < b[campo])
      return -1;
    if (a[campo] > b[campo])
      return 1;

    return 0;
  });
  aggiornaTabella();
}


