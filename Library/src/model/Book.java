package model;

import java.io.Serializable;

/**
 * Created by Elev on 04.02.2017.
 */
public class Book implements Serializable {

    private static final long serialVersionUID = 1L;

    private int id;
    private String title;
    private int price;
    private int copies;

    public Book() {}

    public Book(int id, String title, int price, int copies) {
        this.id = id;
        this.title = title;
        this.price = price;
        this.copies = copies;
    }

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }

    public String getTitle() {
        return title;
    }

    public void setTitle(String title) {
        this.title = title;
    }

    public int getPrice() {
        return price;
    }

    public void setPrice(int price) {
        this.price = price;
    }

    public int getCopies() {
        return copies;
    }

    public void setCopies(int copies) {
        this.copies = copies;
    }
}
