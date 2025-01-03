function validSolution(board) {
    // перевірка чисел на унікальність та чи належать вони проміжку від 1 до 9
    function isValidSet(arr) {
        const nums = new Array(10).fill(false);
        for (const num of arr) {
            if (num < 1 || num > 9) return false; // Перевірка діапазону
            if (nums[num]) return false;         // Перевірка на унікальність
            nums[num] = true;
        }
        return true;
    }

    // Перевірка кожного рядка
    if (!board.every(isValidSet)) return false;

    // Перевірка кожного стовпця
    for (let i = 0; i < 9; i++) {
        const col = board.map(row => row[i]);
        if (!isValidSet(col)) return false;
    }

    // Перевірка квадратиків 3x3
    for (let i = 0; i < 9; i += 3) {
        for (let j = 0; j < 9; j += 3) {
            const block = [];
            for (let x = 0; x < 3; x++) {
                for (let y = 0; y < 3; y++) {
                    block.push(board[i + x][j + y]);
                }
            }
            if (!isValidSet(block)) return false;
        }
    }

    return true; 
}


const board = JSON.parse(process.argv[2]);
console.log(validSolution(board));