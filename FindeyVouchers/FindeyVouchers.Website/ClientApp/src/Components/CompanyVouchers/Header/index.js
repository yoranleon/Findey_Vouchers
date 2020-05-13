import React from 'react';
import {Row, Col} from 'reactstrap';
import {FaMapMarkerAlt, FaGlobe, FaTelegramPlane, FaPhoneSquareAlt} from "react-icons/fa";
import './header.css';

export default class Header extends React.Component {
    constructor(props) {
        super(props);
    }
    render() {
        return (
            <header className="mci-header-container">
                <Row>
                    <Col className="text-center">
            <span className="text-uppercase">
              Overzicht vouchers
            </span>
                    </Col>
                </Row>
                <hr className="mci-header-split"/>
                <Row className="p-4">
                    <Col md="8">
                        <Row>
                            <Col className="text-left">
                                <h1> {this.props.merchant.name} </h1>
                            </Col>
                        </Row>
                        <Row>
                            <Col>
                                {this.props.merchant.description ? this.props.merchant.description : "Hieronder vindt u de vouchers die wij beschikbaar hebben gesteld. Na uw aankoop ontvangt u een e-mail met daarin de vouchers die besteed kunnen worden bij onze onderneming. \n" +
                                    "We willen u alvast bedanken en graag tot snel!" }
                            </Col>
                        </Row>
                    </Col>
                    <Col md="4" className="mci-header-border-l">
                        {this.props.merchant.address ? <Row>
                            <Col sm={{size: 2, offset: 2}} md={{size: 2, offset: 2}}>
                                <FaMapMarkerAlt size={25}/>
                            </Col>
                            <Col>
                                {this.props.merchant.address}
                            </Col>
                        </Row> : ""}

                        <Row className="mt-2">
                            <Col sm={{size: 2, offset: 2}} md={{size: 2, offset: 2}}>
                                <FaGlobe size={25}/>
                            </Col>
                            <Col>
                                {this.props.merchant.website ? this.props.merchant.website : "/"}
                            </Col>
                        </Row>
                        <Row className="mt-2">
                            <Col sm={{size: 2, offset: 2}} md={{size: 2, offset: 2}}>
                                <FaTelegramPlane size={25}/>
                            </Col>
                            <Col>
                                {this.props.merchant.email ? this.props.merchant.email : "/"}
                            </Col>
                        </Row>
                        <Row className="mt-2">
                            <Col sm={{size: 2, offset: 2}} md={{size: 2, offset: 2}}>
                                <FaPhoneSquareAlt size={25}/>
                            </Col>
                            <Col>
                                {this.props.merchant.phoneNumber ? this.props.merchant.phoneNumber : "/"}
                            </Col>
                        </Row>
                    </Col>
                </Row>
            </header>
        );
    }
}