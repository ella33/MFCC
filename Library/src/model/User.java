package model;

import java.io.Serializable;

/**
 * Created by Elev on 04.02.2017.
 */
public class User implements Serializable{
    private static final long serialVersionUID = 1L;

    private int id;
    private String username;

    public User() {
    }

    public User(int id, String username) {
        this.id = id;
        this.username = username;
    }

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }

    public String getUsername() {
        return username;
    }

    public void setUsername(String username) {
        this.username = username;
    }

}
