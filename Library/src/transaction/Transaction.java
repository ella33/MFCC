package transaction;

import java.sql.Timestamp;
import java.util.Stack;

/**
 * Created by Elev on 05.02.2017.
 */
public class Transaction {
    private int id;
    private Timestamp beginTimestamp;
    private int status;

    private Stack<Operation> operations;

    public Transaction() {
    }

    public Transaction(int id, Timestamp beginTimestamp, int status) {
        this.id = id;
        this.beginTimestamp = beginTimestamp;
        this.status = status;
        this.operations = new Stack<>();
    }

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }

    public Timestamp getBeginTimestamp() {
        return beginTimestamp;
    }

    public void setBeginTimestamp(Timestamp beginTimestamp) {
        this.beginTimestamp = beginTimestamp;
    }

    public int getStatus() {
        return status;
    }

    public void setStatus(int status) {
        this.status = status;
    }

    public Stack<Operation> getOperations() {
        return operations;
    }

    @Override
    public boolean equals(Object obj) {
        if(obj instanceof Transaction){
            Transaction other = (Transaction) obj;

            return this.id == other.getId();
        }

        return false;
    }

    @Override
    public String toString() {
        return "Transaction ID = " + id;
    }
}
