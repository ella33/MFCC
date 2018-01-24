package transaction;

/**
 * Created by Elev on 05.02.2017.
 */
public class Lock {
    private Transaction transaction;
    private String table;
    private int lockType;

    public Lock() {
    }

    public Lock(Transaction transaction, String table, int lockType) {
        this.transaction = transaction;
        this.table = table;
        this.lockType = lockType;
    }

    public Transaction getTransaction() {
        return transaction;
    }

    public void setTransaction(Transaction transaction) {
        this.transaction = transaction;
    }

    public String getTable() {
        return table;
    }

    public void setTable(String table) {
        this.table = table;
    }

    public int getLockType() {
        return lockType;
    }

    public void setLockType(int lockType) {
        this.lockType = lockType;
    }
}
